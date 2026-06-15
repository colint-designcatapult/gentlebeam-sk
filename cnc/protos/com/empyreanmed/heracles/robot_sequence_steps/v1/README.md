# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/robot_sequence_steps/v1/robot_sequence_step.proto](#com_empyreanmed_heracles_robot_sequence_steps_v1_robot_sequence_step-proto)
    - [RobotSequenceStep](#com-empyreanmed-heracles-robot_sequence_steps-v1-RobotSequenceStep)
  
- [com/empyreanmed/heracles/robot_sequence_steps/v1/robot_sequence_step_service.proto](#com_empyreanmed_heracles_robot_sequence_steps_v1_robot_sequence_step_service-proto)
    - [CreateRobotSequenceStepRequest](#com-empyreanmed-heracles-robot_sequence_steps-v1-CreateRobotSequenceStepRequest)
    - [CreateRobotSequenceStepResponse](#com-empyreanmed-heracles-robot_sequence_steps-v1-CreateRobotSequenceStepResponse)
    - [DeleteRobotSequenceStepRequest](#com-empyreanmed-heracles-robot_sequence_steps-v1-DeleteRobotSequenceStepRequest)
    - [DeleteRobotSequenceStepResponse](#com-empyreanmed-heracles-robot_sequence_steps-v1-DeleteRobotSequenceStepResponse)
    - [GetRobotSequenceStepRequest](#com-empyreanmed-heracles-robot_sequence_steps-v1-GetRobotSequenceStepRequest)
    - [GetRobotSequenceStepResponse](#com-empyreanmed-heracles-robot_sequence_steps-v1-GetRobotSequenceStepResponse)
    - [ListRobotSequenceStepsRequest](#com-empyreanmed-heracles-robot_sequence_steps-v1-ListRobotSequenceStepsRequest)
    - [ListRobotSequenceStepsResponse](#com-empyreanmed-heracles-robot_sequence_steps-v1-ListRobotSequenceStepsResponse)
    - [UpdateRobotSequenceStepRequest](#com-empyreanmed-heracles-robot_sequence_steps-v1-UpdateRobotSequenceStepRequest)
    - [UpdateRobotSequenceStepResponse](#com-empyreanmed-heracles-robot_sequence_steps-v1-UpdateRobotSequenceStepResponse)
  
    - [RobotSequenceStepService](#com-empyreanmed-heracles-robot_sequence_steps-v1-RobotSequenceStepService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_robot_sequence_steps_v1_robot_sequence_step-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/robot_sequence_steps/v1/robot_sequence_step.proto



<a name="com-empyreanmed-heracles-robot_sequence_steps-v1-RobotSequenceStep"></a>

### RobotSequenceStep
Represents a step in a robot sequence.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the robot sequence step. |
| robot_sequence_id | [int64](#int64) | optional | The ID of the associated robot sequence. |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The creation date of the robot sequence step. |
| step_index | [int32](#int32) | optional | Index of the step in the sequence. |
| action | [string](#string) | optional | The action to be performed in this step. |
| value | [string](#string) | optional | Value associated with the action. |





 

 

 

 



<a name="com_empyreanmed_heracles_robot_sequence_steps_v1_robot_sequence_step_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/robot_sequence_steps/v1/robot_sequence_step_service.proto



<a name="com-empyreanmed-heracles-robot_sequence_steps-v1-CreateRobotSequenceStepRequest"></a>

### CreateRobotSequenceStepRequest
Request message for creating a new robot sequence step.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| robot_sequence_step | [RobotSequenceStep](#com-empyreanmed-heracles-robot_sequence_steps-v1-RobotSequenceStep) | optional | The robot sequence step to be created. |






<a name="com-empyreanmed-heracles-robot_sequence_steps-v1-CreateRobotSequenceStepResponse"></a>

### CreateRobotSequenceStepResponse
Response message with the created robot sequence step.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| robot_sequence_step | [RobotSequenceStep](#com-empyreanmed-heracles-robot_sequence_steps-v1-RobotSequenceStep) | optional | The robot sequence step that was created. |






<a name="com-empyreanmed-heracles-robot_sequence_steps-v1-DeleteRobotSequenceStepRequest"></a>

### DeleteRobotSequenceStepRequest
Request message for deleting a robot sequence step.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the robot sequence step to be deleted. |






<a name="com-empyreanmed-heracles-robot_sequence_steps-v1-DeleteRobotSequenceStepResponse"></a>

### DeleteRobotSequenceStepResponse
An empty response message for `DeleteRobotSequenceStep`.






<a name="com-empyreanmed-heracles-robot_sequence_steps-v1-GetRobotSequenceStepRequest"></a>

### GetRobotSequenceStepRequest
Request message for fetching a single robot sequence step.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the robot sequence step to be returned. |






<a name="com-empyreanmed-heracles-robot_sequence_steps-v1-GetRobotSequenceStepResponse"></a>

### GetRobotSequenceStepResponse
Response message for fetching a single robot sequence step.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| robot_sequence_step | [RobotSequenceStep](#com-empyreanmed-heracles-robot_sequence_steps-v1-RobotSequenceStep) |  | The robot sequence step with the ID provided in the request. |






<a name="com-empyreanmed-heracles-robot_sequence_steps-v1-ListRobotSequenceStepsRequest"></a>

### ListRobotSequenceStepsRequest
Request message for listing robot sequence steps.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| page_size | [int32](#int32) | optional | The maximum number of robot sequence steps to return. |
| page_token | [string](#string) | optional | A page token, received from a previous `ListRobotSequenceSteps` call. |






<a name="com-empyreanmed-heracles-robot_sequence_steps-v1-ListRobotSequenceStepsResponse"></a>

### ListRobotSequenceStepsResponse
Response message with the listed robot sequence steps.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| robot_sequence_steps | [RobotSequenceStep](#com-empyreanmed-heracles-robot_sequence_steps-v1-RobotSequenceStep) | repeated | The robot sequence steps matching the list request. |
| next_page_token | [string](#string) | optional | A token that can be sent as `page_token` to retrieve the next page of results. |






<a name="com-empyreanmed-heracles-robot_sequence_steps-v1-UpdateRobotSequenceStepRequest"></a>

### UpdateRobotSequenceStepRequest
Request message for updating an existing robot sequence step.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| robot_sequence_step | [RobotSequenceStep](#com-empyreanmed-heracles-robot_sequence_steps-v1-RobotSequenceStep) | optional | The robot sequence step to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the robot sequence step to update. Must not be empty. |






<a name="com-empyreanmed-heracles-robot_sequence_steps-v1-UpdateRobotSequenceStepResponse"></a>

### UpdateRobotSequenceStepResponse
Response message with the updated robot sequence step.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| robot_sequence_step | [RobotSequenceStep](#com-empyreanmed-heracles-robot_sequence_steps-v1-RobotSequenceStep) |  | The updated robot sequence step. |





 

 

 


<a name="com-empyreanmed-heracles-robot_sequence_steps-v1-RobotSequenceStepService"></a>

### RobotSequenceStepService
Performs CRUD operations on robot sequence steps.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListRobotSequenceSteps | [ListRobotSequenceStepsRequest](#com-empyreanmed-heracles-robot_sequence_steps-v1-ListRobotSequenceStepsRequest) | [ListRobotSequenceStepsResponse](#com-empyreanmed-heracles-robot_sequence_steps-v1-ListRobotSequenceStepsResponse) | Lists robot sequence steps matching request parameters. |
| GetRobotSequenceStep | [GetRobotSequenceStepRequest](#com-empyreanmed-heracles-robot_sequence_steps-v1-GetRobotSequenceStepRequest) | [GetRobotSequenceStepResponse](#com-empyreanmed-heracles-robot_sequence_steps-v1-GetRobotSequenceStepResponse) | Returns a single robot sequence step. |
| CreateRobotSequenceStep | [CreateRobotSequenceStepRequest](#com-empyreanmed-heracles-robot_sequence_steps-v1-CreateRobotSequenceStepRequest) | [CreateRobotSequenceStepResponse](#com-empyreanmed-heracles-robot_sequence_steps-v1-CreateRobotSequenceStepResponse) | Creates a new robot sequence step. |
| UpdateRobotSequenceStep | [UpdateRobotSequenceStepRequest](#com-empyreanmed-heracles-robot_sequence_steps-v1-UpdateRobotSequenceStepRequest) | [UpdateRobotSequenceStepResponse](#com-empyreanmed-heracles-robot_sequence_steps-v1-UpdateRobotSequenceStepResponse) | Updates a single robot sequence step. |
| DeleteRobotSequenceStep | [DeleteRobotSequenceStepRequest](#com-empyreanmed-heracles-robot_sequence_steps-v1-DeleteRobotSequenceStepRequest) | [DeleteRobotSequenceStepResponse](#com-empyreanmed-heracles-robot_sequence_steps-v1-DeleteRobotSequenceStepResponse) | Deletes a single robot sequence step. |

 



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

