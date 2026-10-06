#include "atmel_start.h"
#include "hardware_backend.h"
#include "uart_backend.h"
#include <array>
#include <cstring>
#include <stdio.h>

#ifdef _WIN32
#include <winsock2.h>
#include <ws2tcpip.h>
#else
#include <errno.h>
#include <fcntl.h>
#include <netinet/in.h>
#include <netinet/tcp.h>
#include <sys/select.h>
#include <sys/socket.h>
#include <unistd.h>
#endif
struct usart_async_descriptor;

class IoDescriptor {
public:
	virtual ~IoDescriptor() = default;
	virtual int32_t write(const uint8_t *const buf, const uint16_t length) = 0;
	virtual int32_t read(uint8_t *const buf, const uint16_t length) = 0;
	virtual void tick() = 0;

    struct usart_async_descriptor *_usart = nullptr;
    usart_cb_t _cb_tx = nullptr;
    usart_cb_t _cb_rx = nullptr;
};

/**
 * \brief I/O descriptor
 */
struct io_descriptor {
	IoDescriptor* desc;
};

struct usart_async_descriptor {
    struct io_descriptor io;
};


extern "C" {

struct usart_async_descriptor HB_UART;
struct usart_async_descriptor HVPS_UART;


int32_t usart_async_register_callback(struct usart_async_descriptor *const descr,
                                      const enum usart_async_callback_type type, usart_cb_t cb)
{
	if (type == USART_ASYNC_RXC_CB) {
		descr->io.desc->_cb_rx = cb;
	} else if (type == USART_ASYNC_TXC_CB) {
		descr->io.desc->_cb_tx = cb;
	}
	return ERR_NONE;
}

int32_t usart_async_enable(struct usart_async_descriptor *const descr)
{
	return ERR_NONE;
}

int32_t usart_async_get_io_descriptor(struct usart_async_descriptor *const descr, struct io_descriptor **io)
{
	*io = &descr->io;
	return ERR_NONE;
}

int32_t io_write(struct io_descriptor *const io_descr, const uint8_t *const buf, const uint16_t length)
{
    return io_descr->desc->write(buf, length);
}

int32_t io_read(struct io_descriptor *const io_descr, uint8_t *const buf, const uint16_t length)
{
	return io_descr->desc->read(buf, length);
}


class UartTcpBackend : public IoDescriptor {
public:
    explicit UartTcpBackend(uint16_t port) : _port(port) {
#ifdef _WIN32
        WSADATA data = {};
        const int startup_error = WSAStartup(MAKEWORD(2, 2), &data);
        if (startup_error != 0) {
            fail("initializing sockets", startup_error);
            return;
        }
        _winsock_started = true;
#endif
        _socket = socket(AF_INET, SOCK_STREAM, 0);
        if (!is_open()) {
            fail("creating socket", socket_error());
            return;
        }

#ifdef _WIN32
        u_long enabled = 1;
        if (ioctlsocket(_socket, FIONBIO, &enabled) != 0) {
#else
        const int flags = fcntl(_socket, F_GETFL, 0);
        if (flags < 0 || fcntl(_socket, F_SETFL, flags | O_NONBLOCK) != 0) {
#endif
            fail("setting nonblocking mode", socket_error());
            return;
        }
        const int enabled_option = 1;
        if (setsockopt(_socket, IPPROTO_TCP, TCP_NODELAY,
                       reinterpret_cast<const char*>(&enabled_option), sizeof(enabled_option)) != 0) {
            fail("setting TCP_NODELAY", socket_error());
            return;
        }
#ifdef SO_NOSIGPIPE
        if (setsockopt(_socket, SOL_SOCKET, SO_NOSIGPIPE,
                       &enabled_option, sizeof(enabled_option)) != 0) {
            fail("disabling SIGPIPE", socket_error());
            return;
        }
#endif
        sockaddr_in address = {};
        address.sin_family = AF_INET;
        address.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
        address.sin_port = htons(_port);
        if (connect(_socket, reinterpret_cast<const sockaddr*>(&address), sizeof(address)) != 0) {
            const int error = socket_error();
#ifdef _WIN32
            if (error != WSAEWOULDBLOCK) {
#else
            if (error != EINPROGRESS) {
#endif
                fail("connecting", error);
                return;
            }
            _connecting = true;
        }
    }

    ~UartTcpBackend() override {
        close_socket();
    }

    int32_t write(const uint8_t* const buf, const uint16_t length) override {
        if (!is_open() || buf == nullptr || length == 0 ||
            length > _tx_buf.size() || _tx_size != 0) {
            return -1;
        }
        std::memcpy(_tx_buf.data(), buf, length);
        _tx_size = length;
        _tx_offset = 0;
        return length;
    }

    int32_t read(uint8_t* const buf, const uint16_t length) override {
        if (buf == nullptr || length == 0 || !_rx_ready) {
            return 0;
        }
        buf[0] = _rx_buf[_rx_offset++];
        _rx_ready = false;
        return 1;
    }

    void tick() override {
        if (!is_open() || (_connecting && !finish_connect())) {
            return;
        }
        flush_transmit();
        receive_available_bytes();
    }

private:
    static int socket_error() {
#ifdef _WIN32
        return WSAGetLastError();
#else
        return errno;
#endif
    }

    static bool would_wait(int error) {
#ifdef _WIN32
        return error == WSAEWOULDBLOCK || error == WSAEINTR;
#else
        return error == EAGAIN || error == EWOULDBLOCK || error == EINTR;
#endif
    }

    bool is_open() const {
#ifdef _WIN32
        return _socket != INVALID_SOCKET;
#else
        return _socket >= 0;
#endif
    }

    void close_socket() {
#ifdef _WIN32
        if (is_open()) {
            closesocket(_socket);
            _socket = INVALID_SOCKET;
        }
        if (_winsock_started) {
            WSACleanup();
            _winsock_started = false;
        }
#else
        if (is_open()) {
            close(_socket);
            _socket = -1;
        }
#endif
        _connecting = false;
        _tx_size = _tx_offset = 0;
        _rx_size = _rx_offset = 0;
        _rx_ready = false;
    }

    void fail(const char* operation, int error) {
        fprintf(stderr, "UART TCP 127.0.0.1:%u: %s (error %d); disconnected\n",
                static_cast<unsigned>(_port), operation, error);
        close_socket();
    }

    bool finish_connect() {
        fd_set writable;
        fd_set errors;
        FD_ZERO(&writable);
        FD_ZERO(&errors);
        FD_SET(_socket, &writable);
        FD_SET(_socket, &errors);
        timeval timeout = {};
#ifdef _WIN32
        const int ready = select(0, nullptr, &writable, &errors, &timeout);
#else
        const int ready = select(_socket + 1, nullptr, &writable, &errors, &timeout);
#endif
        if (ready < 0) {
            const int error = socket_error();
            if (!would_wait(error)) {
                fail("waiting for connection", error);
            }
            return false;
        }
        if (ready == 0) {
            return false;
        }
        int error = 0;
#ifdef _WIN32
        int size = sizeof(error);
#else
        socklen_t size = sizeof(error);
#endif
        if (getsockopt(_socket, SOL_SOCKET, SO_ERROR, reinterpret_cast<char*>(&error), &size) != 0) {
            error = socket_error();
        }
        if (error != 0) {
            fail("connecting", error);
            return false;
        }
        _connecting = false;
        return true;
    }

    void flush_transmit() {
        if (_tx_size == 0) {
            return;
        }
        int flags = 0;
#ifdef MSG_NOSIGNAL
        flags = MSG_NOSIGNAL;
#endif
        const auto sent = send(_socket, reinterpret_cast<const char*>(_tx_buf.data() + _tx_offset),
                               static_cast<int>(_tx_size - _tx_offset), flags);
        if (sent < 0) {
            const int error = socket_error();
            if (!would_wait(error)) {
                fail("sending", error);
            }
            return;
        }
        _tx_offset += static_cast<size_t>(sent);
        if (_tx_offset == _tx_size) {
            _tx_size = _tx_offset = 0;
            if (_cb_tx != nullptr) {
                _cb_tx(_usart);
            }
        }
    }

    void receive_available_bytes() {
        if (!is_open() || _rx_ready) {
            return;
        }
        if (_rx_offset == _rx_size) {
            const auto received = recv(_socket, reinterpret_cast<char*>(_rx_buf.data()),
                                       static_cast<int>(_rx_buf.size()), 0);
            if (received < 0) {
                const int error = socket_error();
                if (!would_wait(error)) {
                    fail("receiving", error);
                }
                return;
            }
            if (received == 0) {
                fail("peer closed connection", 0);
                return;
            }
            _rx_offset = 0;
            _rx_size = static_cast<size_t>(received);
        }
        // TCP packet boundaries are irrelevant: firmware still sees one UART byte per callback.
        while (_rx_offset < _rx_size && !_rx_ready) {
            if (_cb_rx == nullptr) {
                _rx_offset = _rx_size;
                return;
            }
            _rx_ready = true;
            _cb_rx(_usart);
        }
    }

#ifdef _WIN32
    SOCKET _socket = INVALID_SOCKET;
    bool _winsock_started = false;
#else
    int _socket = -1;
#endif
    const uint16_t _port;
    bool _connecting = false;
    std::array<uint8_t, (HB_TX_MSG_SIZE > MAX_HVPS_CMD_BYTES ? HB_TX_MSG_SIZE : MAX_HVPS_CMD_BYTES)> _tx_buf = {};
    size_t _tx_size = 0;
    size_t _tx_offset = 0;
    std::array<uint8_t, 1024> _rx_buf = {};
    size_t _rx_size = 0;
    size_t _rx_offset = 0;
    bool _rx_ready = false;
};

IoDescriptor* uart_create_tcp_descriptor(uint16_t port)
{
    return new UartTcpBackend(port);
}

void uart_destroy_descriptor(IoDescriptor* descriptor)
{
    delete descriptor;
}


void uart_set_descriptor_instance(struct usart_async_descriptor* descr, IoDescriptor* instance)
{
	descr->io.desc = instance;
    instance->_usart = descr;
}

void uart_tick_1ms()
{
	if(HVPS_UART.io.desc != nullptr) {
		HVPS_UART.io.desc->tick();
	}
	if(HB_UART.io.desc != nullptr) {
		HB_UART.io.desc->tick();
	}
}


}