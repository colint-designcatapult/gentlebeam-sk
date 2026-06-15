# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/qcsamples/v1/qcsample.proto](#com_empyreanmed_heracles_qcsamples_v1_qcsample-proto)
    - [QCSample](#com-empyreanmed-heracles-qcsamples-v1-QCSample)
  
- [com/empyreanmed/heracles/qcsamples/v1/qcsample_service.proto](#com_empyreanmed_heracles_qcsamples_v1_qcsample_service-proto)
    - [ApproveQCSampleRequest](#com-empyreanmed-heracles-qcsamples-v1-ApproveQCSampleRequest)
    - [ApproveQCSampleResponse](#com-empyreanmed-heracles-qcsamples-v1-ApproveQCSampleResponse)
    - [CreateQCSampleRequest](#com-empyreanmed-heracles-qcsamples-v1-CreateQCSampleRequest)
    - [CreateQCSampleResponse](#com-empyreanmed-heracles-qcsamples-v1-CreateQCSampleResponse)
    - [DeleteQCSampleRequest](#com-empyreanmed-heracles-qcsamples-v1-DeleteQCSampleRequest)
    - [DeleteQCSampleResponse](#com-empyreanmed-heracles-qcsamples-v1-DeleteQCSampleResponse)
    - [GetQCSampleRequest](#com-empyreanmed-heracles-qcsamples-v1-GetQCSampleRequest)
    - [GetQCSampleResponse](#com-empyreanmed-heracles-qcsamples-v1-GetQCSampleResponse)
    - [ListQCSamplesRequest](#com-empyreanmed-heracles-qcsamples-v1-ListQCSamplesRequest)
    - [ListQCSamplesResponse](#com-empyreanmed-heracles-qcsamples-v1-ListQCSamplesResponse)
    - [UpdateQCSampleRequest](#com-empyreanmed-heracles-qcsamples-v1-UpdateQCSampleRequest)
    - [UpdateQCSampleResponse](#com-empyreanmed-heracles-qcsamples-v1-UpdateQCSampleResponse)
  
    - [QCSampleService](#com-empyreanmed-heracles-qcsamples-v1-QCSampleService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_qcsamples_v1_qcsample-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/qcsamples/v1/qcsample.proto



<a name="com-empyreanmed-heracles-qcsamples-v1-QCSample"></a>

### QCSample
QCSample represents a quality control sample.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | QCSample id, globally unique |
| collimator_configuration_id | [int64](#int64) | optional | The associated collimator configuration id |
| performed_by | [string](#string) | optional | The email of the user who performed the qc sample |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The date the sample was created |
| emission_current | [float](#float) | optional | emission current of the qc sample |
| heater_current | [float](#float) | optional | heater current of the qc sample |
| duration | [float](#float) | optional | The duration of the sample in seconds |
| referenced | [bool](#bool) | optional | Indicates if the sample is referenced |
| notes | [string](#string) | optional | notes about the qc sample |
| approved_by | [string](#string) | optional | The user ID of the user who approved this item. |





 

 

 

 



<a name="com_empyreanmed_heracles_qcsamples_v1_qcsample_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/qcsamples/v1/qcsample_service.proto



<a name="com-empyreanmed-heracles-qcsamples-v1-ApproveQCSampleRequest"></a>

### ApproveQCSampleRequest
Request message for approving a quality control sample.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| username | [string](#string) |  | The username of the user approving the data. |
| password | [string](#string) |  | The password of the user approving the data. |
| qcsample_id | [int64](#int64) |  | The ID of the quality control sample to approve. |






<a name="com-empyreanmed-heracles-qcsamples-v1-ApproveQCSampleResponse"></a>

### ApproveQCSampleResponse
Response message for approving a quality control sample.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| approved_qcsample | [QCSample](#com-empyreanmed-heracles-qcsamples-v1-QCSample) |  | The approved quality control sample. |






<a name="com-empyreanmed-heracles-qcsamples-v1-CreateQCSampleRequest"></a>

### CreateQCSampleRequest
Request message for creating a new quality control sample.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsample | [QCSample](#com-empyreanmed-heracles-qcsamples-v1-QCSample) | optional | Details of the quality control sample to create. |






<a name="com-empyreanmed-heracles-qcsamples-v1-CreateQCSampleResponse"></a>

### CreateQCSampleResponse
Response message with the created quality control sample.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsample | [QCSample](#com-empyreanmed-heracles-qcsamples-v1-QCSample) |  | The quality control sample that was created. |






<a name="com-empyreanmed-heracles-qcsamples-v1-DeleteQCSampleRequest"></a>

### DeleteQCSampleRequest
Request message for deleting a quality control sample.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsample_id | [int64](#int64) | optional | The ID of the quality control sample to delete. |






<a name="com-empyreanmed-heracles-qcsamples-v1-DeleteQCSampleResponse"></a>

### DeleteQCSampleResponse
An empty response message for `DeleteQCSample`.






<a name="com-empyreanmed-heracles-qcsamples-v1-GetQCSampleRequest"></a>

### GetQCSampleRequest
Request message for fetching a single quality control sample.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsample_id | [int64](#int64) | optional | The ID of the quality control sample to fetch. |






<a name="com-empyreanmed-heracles-qcsamples-v1-GetQCSampleResponse"></a>

### GetQCSampleResponse
Response message with the fetched quality control sample.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsample | [QCSample](#com-empyreanmed-heracles-qcsamples-v1-QCSample) |  | The quality control sample with the provided ID. |






<a name="com-empyreanmed-heracles-qcsamples-v1-ListQCSamplesRequest"></a>

### ListQCSamplesRequest
Request message for listing quality control samples.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator_configuration_id | [int64](#int64) | optional | The ID of the collimator_configuration_id for which to list quality control samples. |






<a name="com-empyreanmed-heracles-qcsamples-v1-ListQCSamplesResponse"></a>

### ListQCSamplesResponse
Response message with the listed quality control samples.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsamples | [QCSample](#com-empyreanmed-heracles-qcsamples-v1-QCSample) | repeated | The quality control samples matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-qcsamples-v1-UpdateQCSampleRequest"></a>

### UpdateQCSampleRequest
Request message for updating an existing quality control sample.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsample | [QCSample](#com-empyreanmed-heracles-qcsamples-v1-QCSample) | optional | The quality control sample to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the quality control sample to update. |






<a name="com-empyreanmed-heracles-qcsamples-v1-UpdateQCSampleResponse"></a>

### UpdateQCSampleResponse
Response message with the updated quality control sample.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsample | [QCSample](#com-empyreanmed-heracles-qcsamples-v1-QCSample) |  | The updated quality control sample. |





 

 

 


<a name="com-empyreanmed-heracles-qcsamples-v1-QCSampleService"></a>

### QCSampleService
Performs operations on QCSamples.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListQCSamples | [ListQCSamplesRequest](#com-empyreanmed-heracles-qcsamples-v1-ListQCSamplesRequest) | [ListQCSamplesResponse](#com-empyreanmed-heracles-qcsamples-v1-ListQCSamplesResponse) | Lists quality control samples for a given collimator. |
| GetQCSample | [GetQCSampleRequest](#com-empyreanmed-heracles-qcsamples-v1-GetQCSampleRequest) | [GetQCSampleResponse](#com-empyreanmed-heracles-qcsamples-v1-GetQCSampleResponse) | Returns a single quality control sample. |
| CreateQCSample | [CreateQCSampleRequest](#com-empyreanmed-heracles-qcsamples-v1-CreateQCSampleRequest) | [CreateQCSampleResponse](#com-empyreanmed-heracles-qcsamples-v1-CreateQCSampleResponse) | Creates a new quality control sample. |
| UpdateQCSample | [UpdateQCSampleRequest](#com-empyreanmed-heracles-qcsamples-v1-UpdateQCSampleRequest) | [UpdateQCSampleResponse](#com-empyreanmed-heracles-qcsamples-v1-UpdateQCSampleResponse) | Updates an existing quality control sample. |
| DeleteQCSample | [DeleteQCSampleRequest](#com-empyreanmed-heracles-qcsamples-v1-DeleteQCSampleRequest) | [DeleteQCSampleResponse](#com-empyreanmed-heracles-qcsamples-v1-DeleteQCSampleResponse) | Deletes a quality control sample. |
| ApproveQCSample | [ApproveQCSampleRequest](#com-empyreanmed-heracles-qcsamples-v1-ApproveQCSampleRequest) | [ApproveQCSampleResponse](#com-empyreanmed-heracles-qcsamples-v1-ApproveQCSampleResponse) | Approves a quality control sample. |

 



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

