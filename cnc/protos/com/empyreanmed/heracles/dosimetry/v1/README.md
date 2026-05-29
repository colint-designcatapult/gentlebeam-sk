# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/dosimetry/v1/dosimetry.proto](#com_empyreanmed_heracles_dosimetry_v1_dosimetry-proto)
    - [DosimetryBodyChunk](#com-empyreanmed-heracles-dosimetry-v1-DosimetryBodyChunk)
  
- [com/empyreanmed/heracles/dosimetry/v1/dosimetry_service.proto](#com_empyreanmed_heracles_dosimetry_v1_dosimetry_service-proto)
    - [GetDosimetryBodyRequest](#com-empyreanmed-heracles-dosimetry-v1-GetDosimetryBodyRequest)
    - [GetDosimetryBodyResponse](#com-empyreanmed-heracles-dosimetry-v1-GetDosimetryBodyResponse)
  
    - [DosimetryService](#com-empyreanmed-heracles-dosimetry-v1-DosimetryService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_dosimetry_v1_dosimetry-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/dosimetry/v1/dosimetry.proto



<a name="com-empyreanmed-heracles-dosimetry-v1-DosimetryBodyChunk"></a>

### DosimetryBodyChunk
Chunk of a dosimetry body file


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| data | [bytes](#bytes) |  | Chunk of a dosimetry body file |





 

 

 

 



<a name="com_empyreanmed_heracles_dosimetry_v1_dosimetry_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/dosimetry/v1/dosimetry_service.proto



<a name="com-empyreanmed-heracles-dosimetry-v1-GetDosimetryBodyRequest"></a>

### GetDosimetryBodyRequest
Response message for a header file


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| energy | [string](#string) |  | Can be either 50, 60, 70 etc |
| point | [string](#string) |  | Drift tube target name, e.g A1/D3 for a large head |






<a name="com-empyreanmed-heracles-dosimetry-v1-GetDosimetryBodyResponse"></a>

### GetDosimetryBodyResponse
Response message for a chunk of a body file


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| body_chunk | [DosimetryBodyChunk](#com-empyreanmed-heracles-dosimetry-v1-DosimetryBodyChunk) |  | Single chunk of a dosimetry body file |





 

 

 


<a name="com-empyreanmed-heracles-dosimetry-v1-DosimetryService"></a>

### DosimetryService
This service provies Monte Carlo simulation files for dosimetry calculation purposes

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| GetDosimetryBody | [GetDosimetryBodyRequest](#com-empyreanmed-heracles-dosimetry-v1-GetDosimetryBodyRequest) | [GetDosimetryBodyResponse](#com-empyreanmed-heracles-dosimetry-v1-GetDosimetryBodyResponse) stream | Returns the body file for the given request which contains the actual dosimetry data |

 



## Scalar Value Types

| .proto Type | Notes | C++ | Java | Python | Go | C# | PHP | Ruby |
| ----------- | ----- | --- | ---- | ------ | -- | -- | --- | ---- |
| <a name="double" /> double |  | double | double | float | float64 | double | float | Float |
| <a name="float" /> float |  | float | float | float | float32 | float | float | Float |
| <a name="int32" /> int32 | Uses variable-length encoding. Inefficient for encoding negative numbers – if your field is likely to have negative values, use sint32 instead. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="int64" /> int64 | Uses variable-length encoding. Inefficient for encoding negative numbers – if your field is likely to have negative values, use sint64 instead. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="uint32" /> uint32 | Uses variable-length encoding. | uint32 | int | int/long | uint32 | uint | integer | Bignum or Fixnum (as required) |
| <a name="uint64" /> uint64 | Uses variable-length encoding. | uint64 | long | int/long | uint64 | ulong | integer/string | Bignum or Fixnum (as required) |
| <a name="sint32" /> sint32 | Uses variable-length encoding. Signed int value. These more efficiently encode negative numbers than regular int32s. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="sint64" /> sint64 | Uses variable-length encoding. Signed int value. These more efficiently encode negative numbers than regular int64s. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="fixed32" /> fixed32 | Always four bytes. More efficient than uint32 if values are often greater than 2^28. | uint32 | int | int | uint32 | uint | integer | Bignum or Fixnum (as required) |
| <a name="fixed64" /> fixed64 | Always eight bytes. More efficient than uint64 if values are often greater than 2^56. | uint64 | long | int/long | uint64 | ulong | integer/string | Bignum |
| <a name="sfixed32" /> sfixed32 | Always four bytes. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="sfixed64" /> sfixed64 | Always eight bytes. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="bool" /> bool |  | bool | boolean | boolean | bool | bool | boolean | TrueClass/FalseClass |
| <a name="string" /> string | A string must always contain UTF-8 encoded or 7-bit ASCII text. | string | String | str/unicode | string | string | string | String (UTF-8) |
| <a name="bytes" /> bytes | May contain any arbitrary sequence of bytes. | string | ByteString | str | []byte | ByteString | string | String (ASCII-8BIT) |

