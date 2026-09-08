#include <stdint.h>
#include "base64/base64.h"

static const char b64_enc_table[64] =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

static int b64_dec_value(unsigned char c)
{
    if (c >= 'A' && c <= 'Z') { return (int)(c - 'A'); }
    if (c >= 'a' && c <= 'z') { return (int)(c - 'a' + 26); }
    if (c >= '0' && c <= '9') { return (int)(c - '0' + 52); }
    if (c == '+') { return 62; }
    if (c == '/') { return 63; }
    return -1;
}

int base64_decode_len(const char *in)
{
    int n = 0;

    while (in[n] != '\0' && in[n] != '\n') {
        n++;
    }
    return ((n + 3) / 4) * 3;
}

int base64_decode(const char *in, void *out)
{
    uint8_t *dst = (uint8_t *)out;
    int total = 0;

    for (;;) {
        int value[4];
        int pad = 0;
        int index;

        for (index = 0; index < 4; index++) {
            unsigned char c = (unsigned char)in[index];

            if (c == '\0' || c == '\n') {
                /* Ran out of input mid group. */
                return (index == 0) ? total : -1;
            }
            if (c == '=') {
                value[index] = 0;
                pad++;
            } else {
                int decoded = b64_dec_value(c);
                if (decoded < 0) {
                    return -1;
                }
                value[index] = decoded;
            }
        }
        in += 4;

        dst[total++] = (uint8_t)((value[0] << 2) | (value[1] >> 4));
        if (pad < 2) {
            dst[total++] = (uint8_t)(((value[1] & 0x0Fu) << 4) | (value[2] >> 2));
        }
        if (pad < 1) {
            dst[total++] = (uint8_t)(((value[2] & 0x03u) << 6) | value[3]);
        }

        if (pad > 0 || in[0] == '\0' || in[0] == '\n') {
            break;
        }
    }

    return total;
}

int base64_encode(const void *in, int len, char *out, int add_pad)
{
    const uint8_t *src = (const uint8_t *)in;
    int out_len = 0;
    int index;

    for (index = 0; index + 3 <= len; index += 3) {
        out[out_len++] = b64_enc_table[src[index] >> 2];
        out[out_len++] = b64_enc_table[((src[index] & 0x03u) << 4) | (src[index + 1] >> 4)];
        out[out_len++] = b64_enc_table[((src[index + 1] & 0x0Fu) << 2) | (src[index + 2] >> 6)];
        out[out_len++] = b64_enc_table[src[index + 2] & 0x3Fu];
    }

    if (len - index == 1) {
        out[out_len++] = b64_enc_table[src[index] >> 2];
        out[out_len++] = b64_enc_table[(src[index] & 0x03u) << 4];
        if (add_pad) {
            out[out_len++] = '=';
            out[out_len++] = '=';
        }
    } else if (len - index == 2) {
        out[out_len++] = b64_enc_table[src[index] >> 2];
        out[out_len++] = b64_enc_table[((src[index] & 0x03u) << 4) | (src[index + 1] >> 4)];
        out[out_len++] = b64_enc_table[(src[index + 1] & 0x0Fu) << 2];
        if (add_pad) {
            out[out_len++] = '=';
        }
    }

    out[out_len] = '\0';
    return out_len;
}
