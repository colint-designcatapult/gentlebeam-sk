# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles_robotic_arm/matrices/v1/matrix.proto](#com_empyreanmed_heracles_robotic_arm_matrices_v1_matrix-proto)
    - [Matrix4x4](#com-empyreanmed-heracles_robotic_arm-matrices-v1-Matrix4x4)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_robotic_arm_matrices_v1_matrix-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles_robotic_arm/matrices/v1/matrix.proto



<a name="com-empyreanmed-heracles_robotic_arm-matrices-v1-Matrix4x4"></a>

### Matrix4x4
Message for 4x4 matrix


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| a11 | [float](#float) |  | row 1, column 1 |
| a12 | [float](#float) |  | row 1, column 2 |
| a13 | [float](#float) |  | row 1, column 3 |
| a14 | [float](#float) |  | row 1, column 4 |
| a21 | [float](#float) |  | row 2, column 1 |
| a22 | [float](#float) |  | row 2, column 2 |
| a23 | [float](#float) |  | row 2, column 3 |
| a24 | [float](#float) |  | row 2, column 4 |
| a31 | [float](#float) |  | row 3, column 1 |
| a32 | [float](#float) |  | row 3, column 2 |
| a33 | [float](#float) |  | row 3, column 3 |
| a34 | [float](#float) |  | row 3, column 4 |
| a41 | [float](#float) |  | row 4, column 1 |
| a42 | [float](#float) |  | row 4, column 2 |
| a43 | [float](#float) |  | row 4, column 3 |
| a44 | [float](#float) |  | row 4, column 4 |





 

 

 

 



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

