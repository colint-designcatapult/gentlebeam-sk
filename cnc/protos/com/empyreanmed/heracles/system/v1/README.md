# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/system/v1/system.proto](#com_empyreanmed_heracles_system_v1_system-proto)
    - [System](#com-empyreanmed-heracles-system-v1-System)
  
- [com/empyreanmed/heracles/system/v1/system_service.proto](#com_empyreanmed_heracles_system_v1_system_service-proto)
    - [GetSystemInfoRequest](#com-empyreanmed-heracles-system-v1-GetSystemInfoRequest)
    - [GetSystemInfoResponse](#com-empyreanmed-heracles-system-v1-GetSystemInfoResponse)
  
    - [SystemService](#com-empyreanmed-heracles-system-v1-SystemService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_system_v1_system-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/system/v1/system.proto



<a name="com-empyreanmed-heracles-system-v1-System"></a>

### System
Represents system information


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| version | [string](#string) |  | System version string |





 

 

 

 



<a name="com_empyreanmed_heracles_system_v1_system_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/system/v1/system_service.proto



<a name="com-empyreanmed-heracles-system-v1-GetSystemInfoRequest"></a>

### GetSystemInfoRequest
Request message for getting system information

Empty request






<a name="com-empyreanmed-heracles-system-v1-GetSystemInfoResponse"></a>

### GetSystemInfoResponse
Response message with system information


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| system | [System](#com-empyreanmed-heracles-system-v1-System) |  | System information including version |





 

 

 


<a name="com-empyreanmed-heracles-system-v1-SystemService"></a>

### SystemService
Provides system information and operations

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| GetSystemInfo | [GetSystemInfoRequest](#com-empyreanmed-heracles-system-v1-GetSystemInfoRequest) | [GetSystemInfoResponse](#com-empyreanmed-heracles-system-v1-GetSystemInfoResponse) | Returns system information including version |

 



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

