# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [google/api/field_behavior.proto](#google_api_field_behavior-proto)
    - [FieldBehavior](#google-api-FieldBehavior)
  
    - [File-level Extensions](#google_api_field_behavior-proto-extensions)
  
- [Scalar Value Types](#scalar-value-types)



<a name="google_api_field_behavior-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## google/api/field_behavior.proto


 


<a name="google-api-FieldBehavior"></a>

### FieldBehavior
An indicator of the behavior of a given field (for example, that a field
is required in requests, or given as output but ignored as input).
This **does not** change the behavior in protocol buffers itself; it only
denotes the behavior and may affect how API tooling handles the field.

Note: This enum **may** receive new values in the future.

| Name | Number | Description |
| ---- | ------ | ----------- |
| FIELD_BEHAVIOR_UNSPECIFIED | 0 | Conventional default for enums. Do not use this. |
| OPTIONAL | 1 | Specifically denotes a field as optional. While all fields in protocol buffers are optional, this may be specified for emphasis if appropriate. |
| REQUIRED | 2 | Denotes a field as required. This indicates that the field **must** be provided as part of the request, and failure to do so will cause an error (usually `INVALID_ARGUMENT`). |
| OUTPUT_ONLY | 3 | Denotes a field as output only. This indicates that the field is provided in responses, but including the field in a request does nothing (the server *must* ignore it and *must not* throw an error as a result of the field&#39;s presence). |
| INPUT_ONLY | 4 | Denotes a field as input only. This indicates that the field is provided in requests, and the corresponding field is not included in output. |
| IMMUTABLE | 5 | Denotes a field as immutable. This indicates that the field may be set once in a request to create a resource, but may not be changed thereafter. |
| UNORDERED_LIST | 6 | Denotes that a (repeated) field is an unordered list. This indicates that the service may provide the elements of the list in any arbitrary order, rather than the order the user originally provided. Additionally, the list&#39;s order may or may not be stable. |
| NON_EMPTY_DEFAULT | 7 | Denotes that this field returns a non-empty default value if not set. This indicates that if the user provides the empty value in a request, a non-empty value will be returned. The user will not be aware of what non-empty value to expect. |
| IDENTIFIER | 8 | Denotes that the field in a resource (a message annotated with google.api.resource) is used in the resource name to uniquely identify the resource. For AIP-compliant APIs, this should only be applied to the `name` field on the resource.

This behavior should not be applied to references to other resources within the message.

The identifier field of resources often have different field behavior depending on the request it is embedded in (e.g. for Create methods name is optional and unused, while for Update methods it is required). Instead of method-specific annotations, only `IDENTIFIER` is required. |


 


<a name="google_api_field_behavior-proto-extensions"></a>

### File-level Extensions
| Extension | Type | Base | Number | Description |
| --------- | ---- | ---- | ------ | ----------- |
| field_behavior | FieldBehavior | .google.protobuf.FieldOptions | 1052 | A designation of a specific field behavior (required, output only, etc.) in protobuf messages.

Examples:

 string name = 1 [(google.api.field_behavior) = REQUIRED]; State state = 1 [(google.api.field_behavior) = OUTPUT_ONLY]; google.protobuf.Duration ttl = 1 [(google.api.field_behavior) = INPUT_ONLY]; google.protobuf.Timestamp expire_time = 1 [(google.api.field_behavior) = OUTPUT_ONLY, (google.api.field_behavior) = IMMUTABLE]; |

 

 



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

