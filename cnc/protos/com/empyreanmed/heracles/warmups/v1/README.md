# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/warmups/v1/warmup.proto](#com_empyreanmed_heracles_warmups_v1_warmup-proto)
    - [Warmup](#com-empyreanmed-heracles-warmups-v1-Warmup)
  
- [com/empyreanmed/heracles/warmups/v1/warmup_service.proto](#com_empyreanmed_heracles_warmups_v1_warmup_service-proto)
    - [CreateWarmupRequest](#com-empyreanmed-heracles-warmups-v1-CreateWarmupRequest)
    - [CreateWarmupResponse](#com-empyreanmed-heracles-warmups-v1-CreateWarmupResponse)
    - [DeleteWarmupRequest](#com-empyreanmed-heracles-warmups-v1-DeleteWarmupRequest)
    - [DeleteWarmupResponse](#com-empyreanmed-heracles-warmups-v1-DeleteWarmupResponse)
    - [GetWarmupRequest](#com-empyreanmed-heracles-warmups-v1-GetWarmupRequest)
    - [GetWarmupResponse](#com-empyreanmed-heracles-warmups-v1-GetWarmupResponse)
    - [ListWarmupsRequest](#com-empyreanmed-heracles-warmups-v1-ListWarmupsRequest)
    - [ListWarmupsResponse](#com-empyreanmed-heracles-warmups-v1-ListWarmupsResponse)
    - [UpdateWarmupRequest](#com-empyreanmed-heracles-warmups-v1-UpdateWarmupRequest)
    - [UpdateWarmupResponse](#com-empyreanmed-heracles-warmups-v1-UpdateWarmupResponse)
  
    - [WarmupService](#com-empyreanmed-heracles-warmups-v1-WarmupService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_warmups_v1_warmup-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/warmups/v1/warmup.proto



<a name="com-empyreanmed-heracles-warmups-v1-Warmup"></a>

### Warmup
Warmup represents a warmup configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the warmup. |
| head_id | [int64](#int64) | optional | id of the collimator&#39;s head |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The creation date of the warmup. |
| warmup_type | [com.empyreanmed.heracles.enums.v1.WARMUPTYPE](#com-empyreanmed-heracles-enums-v1-WARMUPTYPE) | optional | Indicates a fast or full warmup. |
| heater_current | [float](#float) | optional | heater_current of a warmup |





 

 

 

 



<a name="com_empyreanmed_heracles_warmups_v1_warmup_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/warmups/v1/warmup_service.proto



<a name="com-empyreanmed-heracles-warmups-v1-CreateWarmupRequest"></a>

### CreateWarmupRequest
Request message for creating a new warmup configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| warmup | [Warmup](#com-empyreanmed-heracles-warmups-v1-Warmup) | optional | The warmup configuration to be created. |






<a name="com-empyreanmed-heracles-warmups-v1-CreateWarmupResponse"></a>

### CreateWarmupResponse
Response message with the created warmup configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| warmup | [Warmup](#com-empyreanmed-heracles-warmups-v1-Warmup) |  | The created warmup configuration. |






<a name="com-empyreanmed-heracles-warmups-v1-DeleteWarmupRequest"></a>

### DeleteWarmupRequest
Request message for deleting a warmup configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the warmup configuration to be deleted. |






<a name="com-empyreanmed-heracles-warmups-v1-DeleteWarmupResponse"></a>

### DeleteWarmupResponse
An empty response message for `DeleteWarmup`.






<a name="com-empyreanmed-heracles-warmups-v1-GetWarmupRequest"></a>

### GetWarmupRequest
Request message for fetching a single warmup configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the warmup configuration to be returned. |






<a name="com-empyreanmed-heracles-warmups-v1-GetWarmupResponse"></a>

### GetWarmupResponse
Response message for fetching a single warmup configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| warmup | [Warmup](#com-empyreanmed-heracles-warmups-v1-Warmup) |  | The warmup configuration with the provided ID. |






<a name="com-empyreanmed-heracles-warmups-v1-ListWarmupsRequest"></a>

### ListWarmupsRequest
Request message for listing warmup configurations.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| page_size | [int32](#int32) | optional | The maximum number of warmup configurations to return. |
| page_token | [string](#string) | optional | A page token received from a previous `ListWarmups` call. |






<a name="com-empyreanmed-heracles-warmups-v1-ListWarmupsResponse"></a>

### ListWarmupsResponse
Response message with the listed warmup configurations.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| warmups | [Warmup](#com-empyreanmed-heracles-warmups-v1-Warmup) | repeated | The warmup configurations matching the list request. |
| next_page_token | [string](#string) | optional | A token to retrieve the next page of results. |






<a name="com-empyreanmed-heracles-warmups-v1-UpdateWarmupRequest"></a>

### UpdateWarmupRequest
Request message for updating an existing warmup configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| warmup | [Warmup](#com-empyreanmed-heracles-warmups-v1-Warmup) | optional | The warmup configuration to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields to update. |






<a name="com-empyreanmed-heracles-warmups-v1-UpdateWarmupResponse"></a>

### UpdateWarmupResponse
Response message with the updated warmup configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| warmup | [Warmup](#com-empyreanmed-heracles-warmups-v1-Warmup) |  | The updated warmup configuration. |





 

 

 


<a name="com-empyreanmed-heracles-warmups-v1-WarmupService"></a>

### WarmupService
Performs CRUD operations on warmup configurations.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListWarmups | [ListWarmupsRequest](#com-empyreanmed-heracles-warmups-v1-ListWarmupsRequest) | [ListWarmupsResponse](#com-empyreanmed-heracles-warmups-v1-ListWarmupsResponse) | Lists warmup configurations matching request parameters. |
| GetWarmup | [GetWarmupRequest](#com-empyreanmed-heracles-warmups-v1-GetWarmupRequest) | [GetWarmupResponse](#com-empyreanmed-heracles-warmups-v1-GetWarmupResponse) | Returns a single warmup configuration. |
| CreateWarmup | [CreateWarmupRequest](#com-empyreanmed-heracles-warmups-v1-CreateWarmupRequest) | [CreateWarmupResponse](#com-empyreanmed-heracles-warmups-v1-CreateWarmupResponse) | Creates a new warmup configuration. |
| UpdateWarmup | [UpdateWarmupRequest](#com-empyreanmed-heracles-warmups-v1-UpdateWarmupRequest) | [UpdateWarmupResponse](#com-empyreanmed-heracles-warmups-v1-UpdateWarmupResponse) | Updates a single warmup configuration. |
| DeleteWarmup | [DeleteWarmupRequest](#com-empyreanmed-heracles-warmups-v1-DeleteWarmupRequest) | [DeleteWarmupResponse](#com-empyreanmed-heracles-warmups-v1-DeleteWarmupResponse) | Deletes a single warmup configuration. |

 



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

