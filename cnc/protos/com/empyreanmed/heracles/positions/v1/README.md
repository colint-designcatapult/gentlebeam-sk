# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/positions/v1/position.proto](#com_empyreanmed_heracles_positions_v1_position-proto)
    - [Position](#com-empyreanmed-heracles-positions-v1-Position)
  
- [com/empyreanmed/heracles/positions/v1/position_service.proto](#com_empyreanmed_heracles_positions_v1_position_service-proto)
    - [CreatePositionRequest](#com-empyreanmed-heracles-positions-v1-CreatePositionRequest)
    - [CreatePositionResponse](#com-empyreanmed-heracles-positions-v1-CreatePositionResponse)
    - [DeletePositionRequest](#com-empyreanmed-heracles-positions-v1-DeletePositionRequest)
    - [DeletePositionResponse](#com-empyreanmed-heracles-positions-v1-DeletePositionResponse)
    - [GetPositionRequest](#com-empyreanmed-heracles-positions-v1-GetPositionRequest)
    - [GetPositionResponse](#com-empyreanmed-heracles-positions-v1-GetPositionResponse)
    - [ListPositionsRequest](#com-empyreanmed-heracles-positions-v1-ListPositionsRequest)
    - [ListPositionsResponse](#com-empyreanmed-heracles-positions-v1-ListPositionsResponse)
    - [UpdatePositionRequest](#com-empyreanmed-heracles-positions-v1-UpdatePositionRequest)
    - [UpdatePositionResponse](#com-empyreanmed-heracles-positions-v1-UpdatePositionResponse)
  
    - [PositionService](#com-empyreanmed-heracles-positions-v1-PositionService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_positions_v1_position-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/positions/v1/position.proto



<a name="com-empyreanmed-heracles-positions-v1-Position"></a>

### Position
Position represents a patient&#39;s position in a simulation.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | ID is the unique identifier for the position. |
| simulation_id | [int64](#int64) | optional | SimulationID is the identifier for the associated simulation. |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | create_date is the date when the position record was created. |
| patient_position | [com.empyreanmed.heracles.enums.v1.POSITION](#com-empyreanmed-heracles-enums-v1-POSITION) | optional | PatientPosition is the position of the patient during the simulation. |





 

 

 

 



<a name="com_empyreanmed_heracles_positions_v1_position_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/positions/v1/position_service.proto



<a name="com-empyreanmed-heracles-positions-v1-CreatePositionRequest"></a>

### CreatePositionRequest
Request message for creating a new position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| position | [Position](#com-empyreanmed-heracles-positions-v1-Position) |  | The details of the position to create. |






<a name="com-empyreanmed-heracles-positions-v1-CreatePositionResponse"></a>

### CreatePositionResponse
Response message with the created position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| position | [Position](#com-empyreanmed-heracles-positions-v1-Position) |  | The newly created position. |






<a name="com-empyreanmed-heracles-positions-v1-DeletePositionRequest"></a>

### DeletePositionRequest
Request message for deleting a position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) |  | The ID of the position to delete. |






<a name="com-empyreanmed-heracles-positions-v1-DeletePositionResponse"></a>

### DeletePositionResponse
Empty response message for delete operations.






<a name="com-empyreanmed-heracles-positions-v1-GetPositionRequest"></a>

### GetPositionRequest
Request message for fetching a single position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) |  | The ID of the position to fetch. |






<a name="com-empyreanmed-heracles-positions-v1-GetPositionResponse"></a>

### GetPositionResponse
Response message with the fetched position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| position | [Position](#com-empyreanmed-heracles-positions-v1-Position) |  | The fetched position. |






<a name="com-empyreanmed-heracles-positions-v1-ListPositionsRequest"></a>

### ListPositionsRequest
list all positions by simulation ID


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) |  | The ID of the simulation for positions list. |






<a name="com-empyreanmed-heracles-positions-v1-ListPositionsResponse"></a>

### ListPositionsResponse
Response message with the fetched Positions.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| positions | [Position](#com-empyreanmed-heracles-positions-v1-Position) | repeated | The fetched positions list. |






<a name="com-empyreanmed-heracles-positions-v1-UpdatePositionRequest"></a>

### UpdatePositionRequest
Request message for updating an existing position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| position | [Position](#com-empyreanmed-heracles-positions-v1-Position) |  | The position to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) |  | A field mask specifying which fields to update. |






<a name="com-empyreanmed-heracles-positions-v1-UpdatePositionResponse"></a>

### UpdatePositionResponse
Response message with the updated position.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| position | [Position](#com-empyreanmed-heracles-positions-v1-Position) |  | The updated position. |





 

 

 


<a name="com-empyreanmed-heracles-positions-v1-PositionService"></a>

### PositionService
PositionService provides CRUD operations for managing patient positions.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListPositions | [ListPositionsRequest](#com-empyreanmed-heracles-positions-v1-ListPositionsRequest) | [ListPositionsResponse](#com-empyreanmed-heracles-positions-v1-ListPositionsResponse) | Retrieves a single position by simulation ID. |
| GetPosition | [GetPositionRequest](#com-empyreanmed-heracles-positions-v1-GetPositionRequest) | [GetPositionResponse](#com-empyreanmed-heracles-positions-v1-GetPositionResponse) | Retrieves a single position by its ID. |
| CreatePosition | [CreatePositionRequest](#com-empyreanmed-heracles-positions-v1-CreatePositionRequest) | [CreatePositionResponse](#com-empyreanmed-heracles-positions-v1-CreatePositionResponse) | Creates a new position. |
| UpdatePosition | [UpdatePositionRequest](#com-empyreanmed-heracles-positions-v1-UpdatePositionRequest) | [UpdatePositionResponse](#com-empyreanmed-heracles-positions-v1-UpdatePositionResponse) | Updates an existing position. |
| DeletePosition | [DeletePositionRequest](#com-empyreanmed-heracles-positions-v1-DeletePositionRequest) | [DeletePositionResponse](#com-empyreanmed-heracles-positions-v1-DeletePositionResponse) | Deletes a position by its ID. |

 



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

