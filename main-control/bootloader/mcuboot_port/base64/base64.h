#ifndef H_BASE64_
#define H_BASE64_

/* Size (including null terminator) of the base64 encoding of an in_size byte
 * buffer; matches mcuboot's boot_serial.c expectations. */
#define BASE64_ENCODE_SIZE(in_size) ((((((in_size) - 1) / 3) * 4) + 4) + 1)

#ifdef __cplusplus
extern "C" {
#endif

/* Mynewt-compatible base64 API used by mcuboot's boot_serial. */
int base64_encode(const void *in, int len, char *out, int add_nl_and_terminate);
int base64_decode(const char *in, void *out);
int base64_decode_len(const char *in);

#ifdef __cplusplus
}
#endif

#endif /* H_BASE64_ */
