# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/head/v1/head.proto](#com_empyreanmed_heracles_head_v1_head-proto)
    - [Head](#com-empyreanmed-heracles-head-v1-Head)
  
- [com/empyreanmed/heracles/head/v1/head_service.proto](#com_empyreanmed_heracles_head_v1_head_service-proto)
    - [CreateHeadRequest](#com-empyreanmed-heracles-head-v1-CreateHeadRequest)
    - [CreateHeadResponse](#com-empyreanmed-heracles-head-v1-CreateHeadResponse)
    - [DeleteHeadRequest](#com-empyreanmed-heracles-head-v1-DeleteHeadRequest)
    - [DeleteHeadResponse](#com-empyreanmed-heracles-head-v1-DeleteHeadResponse)
    - [GetHeadRequest](#com-empyreanmed-heracles-head-v1-GetHeadRequest)
    - [GetHeadResponse](#com-empyreanmed-heracles-head-v1-GetHeadResponse)
    - [ListHeadsRequest](#com-empyreanmed-heracles-head-v1-ListHeadsRequest)
    - [ListHeadsResponse](#com-empyreanmed-heracles-head-v1-ListHeadsResponse)
    - [UpdateHeadRequest](#com-empyreanmed-heracles-head-v1-UpdateHeadRequest)
    - [UpdateHeadResponse](#com-empyreanmed-heracles-head-v1-UpdateHeadResponse)
  
    - [HeadService](#com-empyreanmed-heracles-head-v1-HeadService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_head_v1_head-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/head/v1/head.proto



<a name="com-empyreanmed-heracles-head-v1-Head"></a>

### Head
Represents an head field associated with a head field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | head id, globally unique |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Head creation date |
| serial | [string](#string) | optional | The Serial associated with the Head |
| is_active | [bool](#bool) | optional | activity status of head |





 

 

 

 



<a name="com_empyreanmed_heracles_head_v1_head_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/head/v1/head_service.proto



<a name="com-empyreanmed-heracles-head-v1-CreateHeadRequest"></a>

### CreateHeadRequest
Request message for creating a new head.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| head | [Head](#com-empyreanmed-heracles-head-v1-Head) |  | The head resource to create. |






<a name="com-empyreanmed-heracles-head-v1-CreateHeadResponse"></a>

### CreateHeadResponse
Response message for the created head.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| head | [Head](#com-empyreanmed-heracles-head-v1-Head) |  | The created head details. |






<a name="com-empyreanmed-heracles-head-v1-DeleteHeadRequest"></a>

### DeleteHeadRequest
Request message for deleting a head.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) |  | The unique ID of the head to delete. |






<a name="com-empyreanmed-heracles-head-v1-DeleteHeadResponse"></a>

### DeleteHeadResponse
An empty response message for DeleteHead.






<a name="com-empyreanmed-heracles-head-v1-GetHeadRequest"></a>

### GetHeadRequest
Request message for getting a single head.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) |  | The unique ID of the head to retrieve. |






<a name="com-empyreanmed-heracles-head-v1-GetHeadResponse"></a>

### GetHeadResponse
Response message for a single head.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| head | [Head](#com-empyreanmed-heracles-head-v1-Head) |  | The head details. |






<a name="com-empyreanmed-heracles-head-v1-ListHeadsRequest"></a>

### ListHeadsRequest
Request message for listing heads.






<a name="com-empyreanmed-heracles-head-v1-ListHeadsResponse"></a>

### ListHeadsResponse
Response message containing the list of heads.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| heads | [Head](#com-empyreanmed-heracles-head-v1-Head) | repeated | The list of heads. |






<a name="com-empyreanmed-heracles-head-v1-UpdateHeadRequest"></a>

### UpdateHeadRequest
Request message for updating an existing head.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| head | [Head](#com-empyreanmed-heracles-head-v1-Head) |  | The head resource with updated details. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) |  | A FieldMask specifying which fields to update. |






<a name="com-empyreanmed-heracles-head-v1-UpdateHeadResponse"></a>

### UpdateHeadResponse
Response message for the updated head.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| head | [Head](#com-empyreanmed-heracles-head-v1-Head) |  | The updated head details. |





 

 

 


<a name="com-empyreanmed-heracles-head-v1-HeadService"></a>

### HeadService
Performs operations on Head resources.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListHeads | [ListHeadsRequest](#com-empyreanmed-heracles-head-v1-ListHeadsRequest) | [ListHeadsResponse](#com-empyreanmed-heracles-head-v1-ListHeadsResponse) | Lists all heads available in the system. |
| GetHead | [GetHeadRequest](#com-empyreanmed-heracles-head-v1-GetHeadRequest) | [GetHeadResponse](#com-empyreanmed-heracles-head-v1-GetHeadResponse) | Gets a specific head by its ID. |
| CreateHead | [CreateHeadRequest](#com-empyreanmed-heracles-head-v1-CreateHeadRequest) | [CreateHeadResponse](#com-empyreanmed-heracles-head-v1-CreateHeadResponse) | Creates a new head record. |
| UpdateHead | [UpdateHeadRequest](#com-empyreanmed-heracles-head-v1-UpdateHeadRequest) | [UpdateHeadResponse](#com-empyreanmed-heracles-head-v1-UpdateHeadResponse) | Updates an existing head record. |
| DeleteHead | [DeleteHeadRequest](#com-empyreanmed-heracles-head-v1-DeleteHeadRequest) | [DeleteHeadResponse](#com-empyreanmed-heracles-head-v1-DeleteHeadResponse) | Deletes a head record by ID. |

 



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

