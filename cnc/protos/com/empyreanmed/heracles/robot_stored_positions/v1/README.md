# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/robot_stored_positions/v1/robot_stored_position.proto](#com_empyreanmed_heracles_robot_stored_positions_v1_robot_stored_position-proto)
    - [RobotStoredPosition](#com-empyreanmed-heracles-robot_stored_positions-v1-RobotStoredPosition)
  
- [com/empyreanmed/heracles/robot_stored_positions/v1/robot_stored_position_service.proto](#com_empyreanmed_heracles_robot_stored_positions_v1_robot_stored_position_service-proto)
    - [CreateRobotStoredPositionRequest](#com-empyreanmed-heracles-robot_stored_positions-v1-CreateRobotStoredPositionRequest)
    - [CreateRobotStoredPositionResponse](#com-empyreanmed-heracles-robot_stored_positions-v1-CreateRobotStoredPositionResponse)
    - [DeleteRobotStoredPositionRequest](#com-empyreanmed-heracles-robot_stored_positions-v1-DeleteRobotStoredPositionRequest)
    - [DeleteRobotStoredPositionResponse](#com-empyreanmed-heracles-robot_stored_positions-v1-DeleteRobotStoredPositionResponse)
    - [GetRobotStoredPositionRequest](#com-empyreanmed-heracles-robot_stored_positions-v1-GetRobotStoredPositionRequest)
    - [GetRobotStoredPositionResponse](#com-empyreanmed-heracles-robot_stored_positions-v1-GetRobotStoredPositionResponse)
    - [ListRobotStoredPositionsRequest](#com-empyreanmed-heracles-robot_stored_positions-v1-ListRobotStoredPositionsRequest)
    - [ListRobotStoredPositionsResponse](#com-empyreanmed-heracles-robot_stored_positions-v1-ListRobotStoredPositionsResponse)
    - [UpdateRobotStoredPositionRequest](#com-empyreanmed-heracles-robot_stored_positions-v1-UpdateRobotStoredPositionRequest)
    - [UpdateRobotStoredPositionResponse](#com-empyreanmed-heracles-robot_stored_positions-v1-UpdateRobotStoredPositionResponse)
  
    - [RobotStoredPositionService](#com-empyreanmed-heracles-robot_stored_positions-v1-RobotStoredPositionService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_robot_stored_positions_v1_robot_stored_position-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/robot_stored_positions/v1/robot_stored_position.proto



<a name="com-empyreanmed-heracles-robot_stored_positions-v1-RobotStoredPosition"></a>

### RobotStoredPosition
RobotStoredPosition represents a stored position of a robot.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the robot stored position. |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The creation date of the robot stored position. |
| position_name | [string](#string) | optional | Name of the stored position. |
| j1 | [float](#float) | optional | Joint 1 position value. |
| j2 | [float](#float) | optional | Joint 2 position value. |
| j3 | [float](#float) | optional | Joint 3 position value. |
| j4 | [float](#float) | optional | Joint 4 position value. |
| j5 | [float](#float) | optional | Joint 5 position value. |
| j6 | [float](#float) | optional | Joint 6 position value. |





 

 

 

 



<a name="com_empyreanmed_heracles_robot_stored_positions_v1_robot_stored_position_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/robot_stored_positions/v1/robot_stored_position_service.proto



<a name="com-empyreanmed-heracles-robot_stored_positions-v1-CreateRobotStoredPositionRequest"></a>

### CreateRobotStoredPositionRequest
Request message for creating a new robot stored position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| robot_stored_position | [RobotStoredPosition](#com-empyreanmed-heracles-robot_stored_positions-v1-RobotStoredPosition) | optional | The robot stored position to be created. |






<a name="com-empyreanmed-heracles-robot_stored_positions-v1-CreateRobotStoredPositionResponse"></a>

### CreateRobotStoredPositionResponse
Response message with the created robot stored position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| robot_stored_position | [RobotStoredPosition](#com-empyreanmed-heracles-robot_stored_positions-v1-RobotStoredPosition) |  | The created robot stored position. |






<a name="com-empyreanmed-heracles-robot_stored_positions-v1-DeleteRobotStoredPositionRequest"></a>

### DeleteRobotStoredPositionRequest
Request message for deleting a robot stored position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the robot stored position to be deleted. |






<a name="com-empyreanmed-heracles-robot_stored_positions-v1-DeleteRobotStoredPositionResponse"></a>

### DeleteRobotStoredPositionResponse
An empty response message for `DeleteRobotStoredPosition`.






<a name="com-empyreanmed-heracles-robot_stored_positions-v1-GetRobotStoredPositionRequest"></a>

### GetRobotStoredPositionRequest
Request message for fetching a single robot stored position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the robot stored position to be returned. |






<a name="com-empyreanmed-heracles-robot_stored_positions-v1-GetRobotStoredPositionResponse"></a>

### GetRobotStoredPositionResponse
Response message for fetching a single robot stored position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| robot_stored_position | [RobotStoredPosition](#com-empyreanmed-heracles-robot_stored_positions-v1-RobotStoredPosition) |  | The robot stored position with the provided ID. |






<a name="com-empyreanmed-heracles-robot_stored_positions-v1-ListRobotStoredPositionsRequest"></a>

### ListRobotStoredPositionsRequest
Request message for listing robot stored positions.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| page_size | [int32](#int32) | optional | The maximum number of robot stored positions to return. |
| page_token | [string](#string) | optional | A page token received from a previous `ListRobotStoredPositions` call. |






<a name="com-empyreanmed-heracles-robot_stored_positions-v1-ListRobotStoredPositionsResponse"></a>

### ListRobotStoredPositionsResponse
Response message with the listed robot stored positions.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| robot_stored_positions | [RobotStoredPosition](#com-empyreanmed-heracles-robot_stored_positions-v1-RobotStoredPosition) | repeated | The robot stored positions matching the list request. |
| next_page_token | [string](#string) | optional | A token to retrieve the next page of results. |






<a name="com-empyreanmed-heracles-robot_stored_positions-v1-UpdateRobotStoredPositionRequest"></a>

### UpdateRobotStoredPositionRequest
Request message for updating an existing robot stored position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| robot_stored_position | [RobotStoredPosition](#com-empyreanmed-heracles-robot_stored_positions-v1-RobotStoredPosition) | optional | The robot stored position to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields to update. |






<a name="com-empyreanmed-heracles-robot_stored_positions-v1-UpdateRobotStoredPositionResponse"></a>

### UpdateRobotStoredPositionResponse
Response message with the updated robot stored position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| robot_stored_position | [RobotStoredPosition](#com-empyreanmed-heracles-robot_stored_positions-v1-RobotStoredPosition) |  | The updated robot stored position. |





 

 

 


<a name="com-empyreanmed-heracles-robot_stored_positions-v1-RobotStoredPositionService"></a>

### RobotStoredPositionService
Performs CRUD operations on robot stored positions.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListRobotStoredPositions | [ListRobotStoredPositionsRequest](#com-empyreanmed-heracles-robot_stored_positions-v1-ListRobotStoredPositionsRequest) | [ListRobotStoredPositionsResponse](#com-empyreanmed-heracles-robot_stored_positions-v1-ListRobotStoredPositionsResponse) | Lists robot stored positions matching request parameters. |
| GetRobotStoredPosition | [GetRobotStoredPositionRequest](#com-empyreanmed-heracles-robot_stored_positions-v1-GetRobotStoredPositionRequest) | [GetRobotStoredPositionResponse](#com-empyreanmed-heracles-robot_stored_positions-v1-GetRobotStoredPositionResponse) | Returns a single robot stored position. |
| CreateRobotStoredPosition | [CreateRobotStoredPositionRequest](#com-empyreanmed-heracles-robot_stored_positions-v1-CreateRobotStoredPositionRequest) | [CreateRobotStoredPositionResponse](#com-empyreanmed-heracles-robot_stored_positions-v1-CreateRobotStoredPositionResponse) | Creates a new robot stored position. |
| UpdateRobotStoredPosition | [UpdateRobotStoredPositionRequest](#com-empyreanmed-heracles-robot_stored_positions-v1-UpdateRobotStoredPositionRequest) | [UpdateRobotStoredPositionResponse](#com-empyreanmed-heracles-robot_stored_positions-v1-UpdateRobotStoredPositionResponse) | Updates a single robot stored position. |
| DeleteRobotStoredPosition | [DeleteRobotStoredPositionRequest](#com-empyreanmed-heracles-robot_stored_positions-v1-DeleteRobotStoredPositionRequest) | [DeleteRobotStoredPositionResponse](#com-empyreanmed-heracles-robot_stored_positions-v1-DeleteRobotStoredPositionResponse) | Deletes a single robot stored position. |

 



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

