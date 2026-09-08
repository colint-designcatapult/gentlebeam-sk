#ifndef BASE64_H
#define BASE64_H

/* boot_serial.c only defines this for __ZEPHYR__/__ESPRESSIF__; supply it
 * here since this header is included by the bare-metal build too. */
#ifndef BASE64_ENCODE_SIZE
#define BASE64_ENCODE_SIZE(in_size) ((((((in_size) - 1) / 3) * 4) + 4) + 1)
#endif

/* Scans a NUL/newline-terminated base64 run and returns an upper bound
 * (in bytes) on the decoded output length. */
int base64_decode_len(const char *in);

/* Decodes a NUL/newline-terminated base64 run into out. Returns the number
 * of decoded bytes, or -1 on malformed input. */
int base64_decode(const char *in, void *out);

/* Encodes len bytes from in into out as base64, NUL-terminating the result.
 * Appends '=' padding when add_pad is non-zero. Returns the encoded length
 * (excluding the terminating NUL). */
int base64_encode(const void *in, int len, char *out, int add_pad);

#endif
