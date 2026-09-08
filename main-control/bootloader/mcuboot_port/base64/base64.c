#include "base64/base64.h"

#include <stddef.h>
#include <stdint.h>

static const char b64_table[] =
	"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

static int8_t b64_rank(unsigned char c)
{
	if (c >= 'A' && c <= 'Z') {
		return (int8_t)(c - 'A');
	}
	if (c >= 'a' && c <= 'z') {
		return (int8_t)(c - 'a' + 26);
	}
	if (c >= '0' && c <= '9') {
		return (int8_t)(c - '0' + 52);
	}
	if (c == '+') {
		return 62;
	}
	if (c == '/') {
		return 63;
	}
	return -1;
}

int base64_encode(const void *in, int len, char *out, int add_nl_and_terminate)
{
	const uint8_t *src = (const uint8_t *)in;
	int outlen = 0;
	int i = 0;

	while (i + 3 <= len) {
		uint32_t v = ((uint32_t)src[i] << 16) | ((uint32_t)src[i + 1] << 8) | src[i + 2];
		out[outlen++] = b64_table[(v >> 18) & 0x3f];
		out[outlen++] = b64_table[(v >> 12) & 0x3f];
		out[outlen++] = b64_table[(v >> 6) & 0x3f];
		out[outlen++] = b64_table[v & 0x3f];
		i += 3;
	}

	if (len - i == 1) {
		uint32_t v = (uint32_t)src[i] << 16;
		out[outlen++] = b64_table[(v >> 18) & 0x3f];
		out[outlen++] = b64_table[(v >> 12) & 0x3f];
		out[outlen++] = '=';
		out[outlen++] = '=';
	} else if (len - i == 2) {
		uint32_t v = ((uint32_t)src[i] << 16) | ((uint32_t)src[i + 1] << 8);
		out[outlen++] = b64_table[(v >> 18) & 0x3f];
		out[outlen++] = b64_table[(v >> 12) & 0x3f];
		out[outlen++] = b64_table[(v >> 6) & 0x3f];
		out[outlen++] = '=';
	}

	if (add_nl_and_terminate) {
		out[outlen] = '\0';
	}

	return outlen;
}

int base64_decode_len(const char *in)
{
	int len = 0;
	int pad = 0;

	while (in[len] != '\0' && in[len] != '\n' && in[len] != '\r') {
		len++;
	}
	if (len >= 1 && in[len - 1] == '=') {
		pad++;
	}
	if (len >= 2 && in[len - 2] == '=') {
		pad++;
	}

	return (len / 4) * 3 - pad;
}

int base64_decode(const char *in, void *out)
{
	uint8_t *dst = (uint8_t *)out;
	int outlen = 0;
	int group[4];
	int gi = 0;

	for (; *in != '\0' && *in != '\n' && *in != '\r'; in++) {
		if (*in == '=') {
			continue;
		}
		int8_t rank = b64_rank((unsigned char)*in);
		if (rank < 0) {
			return -1;
		}
		group[gi++] = rank;
		if (gi == 4) {
			dst[outlen++] = (uint8_t)((group[0] << 2) | (group[1] >> 4));
			dst[outlen++] = (uint8_t)((group[1] << 4) | (group[2] >> 2));
			dst[outlen++] = (uint8_t)((group[2] << 6) | group[3]);
			gi = 0;
		}
	}

	if (gi == 2) {
		dst[outlen++] = (uint8_t)((group[0] << 2) | (group[1] >> 4));
	} else if (gi == 3) {
		dst[outlen++] = (uint8_t)((group[0] << 2) | (group[1] >> 4));
		dst[outlen++] = (uint8_t)((group[1] << 4) | (group[2] >> 2));
	}

	return outlen;
}
