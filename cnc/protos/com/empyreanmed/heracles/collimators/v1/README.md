# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/collimators/v1/collimator.proto](#com_empyreanmed_heracles_collimators_v1_collimator-proto)
    - [Collimator](#com-empyreanmed-heracles-collimators-v1-Collimator)
  
- [com/empyreanmed/heracles/collimators/v1/collimator_service.proto](#com_empyreanmed_heracles_collimators_v1_collimator_service-proto)
    - [CreateCollimatorRequest](#com-empyreanmed-heracles-collimators-v1-CreateCollimatorRequest)
    - [CreateCollimatorResponse](#com-empyreanmed-heracles-collimators-v1-CreateCollimatorResponse)
    - [DeleteCollimatorRequest](#com-empyreanmed-heracles-collimators-v1-DeleteCollimatorRequest)
    - [DeleteCollimatorResponse](#com-empyreanmed-heracles-collimators-v1-DeleteCollimatorResponse)
    - [GetCollimatorRequest](#com-empyreanmed-heracles-collimators-v1-GetCollimatorRequest)
    - [GetCollimatorResponse](#com-empyreanmed-heracles-collimators-v1-GetCollimatorResponse)
    - [ListCollimatorsRequest](#com-empyreanmed-heracles-collimators-v1-ListCollimatorsRequest)
    - [ListCollimatorsResponse](#com-empyreanmed-heracles-collimators-v1-ListCollimatorsResponse)
    - [UpdateCollimatorRequest](#com-empyreanmed-heracles-collimators-v1-UpdateCollimatorRequest)
    - [UpdateCollimatorResponse](#com-empyreanmed-heracles-collimators-v1-UpdateCollimatorResponse)
  
    - [CollimatorService](#com-empyreanmed-heracles-collimators-v1-CollimatorService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_collimators_v1_collimator-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/collimators/v1/collimator.proto



<a name="com-empyreanmed-heracles-collimators-v1-Collimator"></a>

### Collimator
Target represents a collimator` entity in the system.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the collimator. |
| head_id | [int64](#int64) | optional | id of the collimator&#39;s head |
| collimator_configuration_id | [int64](#int64) | optional | id of the collimator&#39;s configuration. |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The creation date of the collimator. |
| serial | [string](#string) | optional | Serial number of the collimator. |
| is_active | [bool](#bool) | optional | Indicates if the collimator is active. |





 

 

 

 



<a name="com_empyreanmed_heracles_collimators_v1_collimator_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/collimators/v1/collimator_service.proto



<a name="com-empyreanmed-heracles-collimators-v1-CreateCollimatorRequest"></a>

### CreateCollimatorRequest
Request message for creating a new collimator.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator | [Collimator](#com-empyreanmed-heracles-collimators-v1-Collimator) | optional | The collimator to be created. |






<a name="com-empyreanmed-heracles-collimators-v1-CreateCollimatorResponse"></a>

### CreateCollimatorResponse
Response message with the created collimator.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator | [Collimator](#com-empyreanmed-heracles-collimators-v1-Collimator) | optional | The collimator that was created. |






<a name="com-empyreanmed-heracles-collimators-v1-DeleteCollimatorRequest"></a>

### DeleteCollimatorRequest
Request message for deleting a collimator.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the collimator to be deleted. |






<a name="com-empyreanmed-heracles-collimators-v1-DeleteCollimatorResponse"></a>

### DeleteCollimatorResponse
An empty response message for `DeleteCollimator`.






<a name="com-empyreanmed-heracles-collimators-v1-GetCollimatorRequest"></a>

### GetCollimatorRequest
Request message for fetching a single collimator.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| serial | [string](#string) | optional | The Serial of the collimator to be returned. |






<a name="com-empyreanmed-heracles-collimators-v1-GetCollimatorResponse"></a>

### GetCollimatorResponse
Response message for fetching a single collimator.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator | [Collimator](#com-empyreanmed-heracles-collimators-v1-Collimator) |  | The collimator with the ID provided in the request. |






<a name="com-empyreanmed-heracles-collimators-v1-ListCollimatorsRequest"></a>

### ListCollimatorsRequest
Request message for listing collimators.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| page_size | [int32](#int32) | optional | The maximum number of collimators to return. The service may return fewer than this value. If unset or zero, all collimators will be returned. |
| page_token | [string](#string) | optional | A page token, received from a previous `ListCollimators` call. Provide this to retrieve the subsequent page.

When paginating, all other parameters provided to `ListCollimators` must match the call that provided the page token. |
| collimator_configuration_id | [int64](#int64) | optional | The head ID to filter collimators by. |






<a name="com-empyreanmed-heracles-collimators-v1-ListCollimatorsResponse"></a>

### ListCollimatorsResponse
Response message with the listed collimators.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimators | [Collimator](#com-empyreanmed-heracles-collimators-v1-Collimator) | repeated | The collimators matching the list request. The order is unspecified. |
| next_page_token | [string](#string) | optional | A token that can be sent as `page_token` to retrieve the next page of results. If this field is omitted, there are no more results. |






<a name="com-empyreanmed-heracles-collimators-v1-UpdateCollimatorRequest"></a>

### UpdateCollimatorRequest
Request message for updating an existing collimator.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator | [Collimator](#com-empyreanmed-heracles-collimators-v1-Collimator) | optional | The collimator to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the collimator to update. Must not be empty. Fields that have `OUTPUT_ONLY` behavior may not be updated. |






<a name="com-empyreanmed-heracles-collimators-v1-UpdateCollimatorResponse"></a>

### UpdateCollimatorResponse
Response message with the updated collimator.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator | [Collimator](#com-empyreanmed-heracles-collimators-v1-Collimator) |  | The updated collimator. |





 

 

 


<a name="com-empyreanmed-heracles-collimators-v1-CollimatorService"></a>

### CollimatorService
Performs CRUD operations on collimators.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListCollimators | [ListCollimatorsRequest](#com-empyreanmed-heracles-collimators-v1-ListCollimatorsRequest) | [ListCollimatorsResponse](#com-empyreanmed-heracles-collimators-v1-ListCollimatorsResponse) | Lists collimators matching request parameters. |
| GetCollimator | [GetCollimatorRequest](#com-empyreanmed-heracles-collimators-v1-GetCollimatorRequest) | [GetCollimatorResponse](#com-empyreanmed-heracles-collimators-v1-GetCollimatorResponse) | Returns a single collimator. |
| CreateCollimator | [CreateCollimatorRequest](#com-empyreanmed-heracles-collimators-v1-CreateCollimatorRequest) | [CreateCollimatorResponse](#com-empyreanmed-heracles-collimators-v1-CreateCollimatorResponse) | Creates a new collimator. |
| UpdateCollimator | [UpdateCollimatorRequest](#com-empyreanmed-heracles-collimators-v1-UpdateCollimatorRequest) | [UpdateCollimatorResponse](#com-empyreanmed-heracles-collimators-v1-UpdateCollimatorResponse) | Updates a single collimator. |
| DeleteCollimator | [DeleteCollimatorRequest](#com-empyreanmed-heracles-collimators-v1-DeleteCollimatorRequest) | [DeleteCollimatorResponse](#com-empyreanmed-heracles-collimators-v1-DeleteCollimatorResponse) | Deletes a single collimator. |

 



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

