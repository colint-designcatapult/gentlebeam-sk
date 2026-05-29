# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/safety_checks/v1/safety_check.proto](#com_empyreanmed_heracles_safety_checks_v1_safety_check-proto)
    - [SafetyCheck](#com-empyreanmed-heracles-safety_checks-v1-SafetyCheck)
  
- [com/empyreanmed/heracles/safety_checks/v1/safety_checks_service.proto](#com_empyreanmed_heracles_safety_checks_v1_safety_checks_service-proto)
    - [CreateSafetyCheckRequest](#com-empyreanmed-heracles-safety_checks-v1-CreateSafetyCheckRequest)
    - [CreateSafetyCheckResponse](#com-empyreanmed-heracles-safety_checks-v1-CreateSafetyCheckResponse)
    - [DeleteSafetyCheckRequest](#com-empyreanmed-heracles-safety_checks-v1-DeleteSafetyCheckRequest)
    - [DeleteSafetyCheckResponse](#com-empyreanmed-heracles-safety_checks-v1-DeleteSafetyCheckResponse)
    - [GetSafetyCheckRequest](#com-empyreanmed-heracles-safety_checks-v1-GetSafetyCheckRequest)
    - [GetSafetyCheckResponse](#com-empyreanmed-heracles-safety_checks-v1-GetSafetyCheckResponse)
    - [ListSafetyChecksRequest](#com-empyreanmed-heracles-safety_checks-v1-ListSafetyChecksRequest)
    - [ListSafetyChecksResponse](#com-empyreanmed-heracles-safety_checks-v1-ListSafetyChecksResponse)
    - [UpdateSafetyCheckRequest](#com-empyreanmed-heracles-safety_checks-v1-UpdateSafetyCheckRequest)
    - [UpdateSafetyCheckResponse](#com-empyreanmed-heracles-safety_checks-v1-UpdateSafetyCheckResponse)
  
    - [SafetyCheckService](#com-empyreanmed-heracles-safety_checks-v1-SafetyCheckService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_safety_checks_v1_safety_check-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/safety_checks/v1/safety_check.proto



<a name="com-empyreanmed-heracles-safety_checks-v1-SafetyCheck"></a>

### SafetyCheck
Represents a safety check


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the safety check |
| performed_by | [string](#string) | optional | The email address of the person who performed the safety check |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Date and time when the safety check was created |
| energy | [com.empyreanmed.heracles.enums.v1.ENERGY](#com-empyreanmed-heracles-enums-v1-ENERGY) | optional | Energy level used during the safety check |
| duration | [float](#float) | optional | Duration of the safety check in seconds |
| dose | [float](#float) | optional | Dose measured during the safety check |
| x_ray_light | [bool](#bool) | optional | Indicates whether x-ray light was on during the check |
| x_ray_sound | [bool](#bool) | optional | Indicates whether x-ray sound was on during the check |
| door_interlock | [bool](#bool) | optional | Indicates whether door interlock was active during the check |
| e_stop | [bool](#bool) | optional | Indicates whether emergency stop (E-stop) was activated |
| s_stop | [bool](#bool) | optional | Indicates whether service stop (S-stop) was activated |
| live_video | [bool](#bool) | optional | Indicates whether live video was available during the safety check |
| live_audio | [bool](#bool) | optional | Indicates whether live audio was available during the safety check |





 

 

 

 



<a name="com_empyreanmed_heracles_safety_checks_v1_safety_checks_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/safety_checks/v1/safety_checks_service.proto



<a name="com-empyreanmed-heracles-safety_checks-v1-CreateSafetyCheckRequest"></a>

### CreateSafetyCheckRequest
Request message for creating a new safety check


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| safety_check | [SafetyCheck](#com-empyreanmed-heracles-safety_checks-v1-SafetyCheck) |  | Safety check to create |






<a name="com-empyreanmed-heracles-safety_checks-v1-CreateSafetyCheckResponse"></a>

### CreateSafetyCheckResponse
Response message for creating a new safety check


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| safety_check | [SafetyCheck](#com-empyreanmed-heracles-safety_checks-v1-SafetyCheck) |  | The created safety check |






<a name="com-empyreanmed-heracles-safety_checks-v1-DeleteSafetyCheckRequest"></a>

### DeleteSafetyCheckRequest
Request message for deleting a safety check by ID


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) |  | ID of the safety check to delete |






<a name="com-empyreanmed-heracles-safety_checks-v1-DeleteSafetyCheckResponse"></a>

### DeleteSafetyCheckResponse
Response message for deleting a safety check






<a name="com-empyreanmed-heracles-safety_checks-v1-GetSafetyCheckRequest"></a>

### GetSafetyCheckRequest
Request message for getting a single safety check by ID


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) |  | ID of the requested safety check |






<a name="com-empyreanmed-heracles-safety_checks-v1-GetSafetyCheckResponse"></a>

### GetSafetyCheckResponse
Response message for getting a single safety check


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| safety_check | [SafetyCheck](#com-empyreanmed-heracles-safety_checks-v1-SafetyCheck) |  | The requested safety check |






<a name="com-empyreanmed-heracles-safety_checks-v1-ListSafetyChecksRequest"></a>

### ListSafetyChecksRequest
Request message for listing safety checks






<a name="com-empyreanmed-heracles-safety_checks-v1-ListSafetyChecksResponse"></a>

### ListSafetyChecksResponse
Response message for listing safety checks


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| safety_checks | [SafetyCheck](#com-empyreanmed-heracles-safety_checks-v1-SafetyCheck) | repeated | List of safety checks |






<a name="com-empyreanmed-heracles-safety_checks-v1-UpdateSafetyCheckRequest"></a>

### UpdateSafetyCheckRequest
Request message for updating an existing safety check


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| safety_check | [SafetyCheck](#com-empyreanmed-heracles-safety_checks-v1-SafetyCheck) |  | Safety check with updates |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields to update. |






<a name="com-empyreanmed-heracles-safety_checks-v1-UpdateSafetyCheckResponse"></a>

### UpdateSafetyCheckResponse
Response message for updating an existing safety check


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| safety_check | [SafetyCheck](#com-empyreanmed-heracles-safety_checks-v1-SafetyCheck) |  | The updated safety check |





 

 

 


<a name="com-empyreanmed-heracles-safety_checks-v1-SafetyCheckService"></a>

### SafetyCheckService
Service definition for SafetyCheck

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListSafetyChecks | [ListSafetyChecksRequest](#com-empyreanmed-heracles-safety_checks-v1-ListSafetyChecksRequest) | [ListSafetyChecksResponse](#com-empyreanmed-heracles-safety_checks-v1-ListSafetyChecksResponse) | RPC method to list all safety checks |
| GetSafetyCheck | [GetSafetyCheckRequest](#com-empyreanmed-heracles-safety_checks-v1-GetSafetyCheckRequest) | [GetSafetyCheckResponse](#com-empyreanmed-heracles-safety_checks-v1-GetSafetyCheckResponse) | RPC method to get a single safety check by ID |
| CreateSafetyCheck | [CreateSafetyCheckRequest](#com-empyreanmed-heracles-safety_checks-v1-CreateSafetyCheckRequest) | [CreateSafetyCheckResponse](#com-empyreanmed-heracles-safety_checks-v1-CreateSafetyCheckResponse) | RPC method to create a new safety check |
| UpdateSafetyCheck | [UpdateSafetyCheckRequest](#com-empyreanmed-heracles-safety_checks-v1-UpdateSafetyCheckRequest) | [UpdateSafetyCheckResponse](#com-empyreanmed-heracles-safety_checks-v1-UpdateSafetyCheckResponse) | RPC method to update an existing safety check |
| DeleteSafetyCheck | [DeleteSafetyCheckRequest](#com-empyreanmed-heracles-safety_checks-v1-DeleteSafetyCheckRequest) | [DeleteSafetyCheckResponse](#com-empyreanmed-heracles-safety_checks-v1-DeleteSafetyCheckResponse) | RPC method to delete a safety check by ID |

 



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

