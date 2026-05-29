# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/robot_sequences/v1/robot_sequence.proto](#com_empyreanmed_heracles_robot_sequences_v1_robot_sequence-proto)
    - [RobotSequence](#com-empyreanmed-heracles-robot_sequences-v1-RobotSequence)
  
- [com/empyreanmed/heracles/robot_sequences/v1/robot_sequence_service.proto](#com_empyreanmed_heracles_robot_sequences_v1_robot_sequence_service-proto)
    - [CreateRobotSequenceRequest](#com-empyreanmed-heracles-robot_sequences-v1-CreateRobotSequenceRequest)
    - [CreateRobotSequenceResponse](#com-empyreanmed-heracles-robot_sequences-v1-CreateRobotSequenceResponse)
    - [DeleteRobotSequenceRequest](#com-empyreanmed-heracles-robot_sequences-v1-DeleteRobotSequenceRequest)
    - [DeleteRobotSequenceResponse](#com-empyreanmed-heracles-robot_sequences-v1-DeleteRobotSequenceResponse)
    - [GetRobotSequenceRequest](#com-empyreanmed-heracles-robot_sequences-v1-GetRobotSequenceRequest)
    - [GetRobotSequenceResponse](#com-empyreanmed-heracles-robot_sequences-v1-GetRobotSequenceResponse)
    - [ListRobotSequencesRequest](#com-empyreanmed-heracles-robot_sequences-v1-ListRobotSequencesRequest)
    - [ListRobotSequencesResponse](#com-empyreanmed-heracles-robot_sequences-v1-ListRobotSequencesResponse)
    - [UpdateRobotSequenceRequest](#com-empyreanmed-heracles-robot_sequences-v1-UpdateRobotSequenceRequest)
    - [UpdateRobotSequenceResponse](#com-empyreanmed-heracles-robot_sequences-v1-UpdateRobotSequenceResponse)
  
    - [RobotSequenceService](#com-empyreanmed-heracles-robot_sequences-v1-RobotSequenceService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_robot_sequences_v1_robot_sequence-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/robot_sequences/v1/robot_sequence.proto



<a name="com-empyreanmed-heracles-robot_sequences-v1-RobotSequence"></a>

### RobotSequence
RobotSequence represents a robot sequence entity in the system.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the robot sequence. |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The creation date of the robot sequence. |
| sequences_name | [string](#string) | optional | Name of the sequence. |





 

 

 

 



<a name="com_empyreanmed_heracles_robot_sequences_v1_robot_sequence_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/robot_sequences/v1/robot_sequence_service.proto



<a name="com-empyreanmed-heracles-robot_sequences-v1-CreateRobotSequenceRequest"></a>

### CreateRobotSequenceRequest
Request message for creating a new robot sequence.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| sequence | [RobotSequence](#com-empyreanmed-heracles-robot_sequences-v1-RobotSequence) | optional | Details of the robot sequence to create. |






<a name="com-empyreanmed-heracles-robot_sequences-v1-CreateRobotSequenceResponse"></a>

### CreateRobotSequenceResponse
Response message with the created robot sequence.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| sequence | [RobotSequence](#com-empyreanmed-heracles-robot_sequences-v1-RobotSequence) |  | The robot sequence that was created. |






<a name="com-empyreanmed-heracles-robot_sequences-v1-DeleteRobotSequenceRequest"></a>

### DeleteRobotSequenceRequest
Request message for deleting a robot sequence.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| sequence_id | [int64](#int64) | optional | The ID of the robot sequence to delete. |






<a name="com-empyreanmed-heracles-robot_sequences-v1-DeleteRobotSequenceResponse"></a>

### DeleteRobotSequenceResponse
An empty response message for `DeleteRobotSequence`.






<a name="com-empyreanmed-heracles-robot_sequences-v1-GetRobotSequenceRequest"></a>

### GetRobotSequenceRequest
Request message for fetching a single robot sequence.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| sequence_id | [int64](#int64) | optional | The ID of the robot sequence to fetch. |






<a name="com-empyreanmed-heracles-robot_sequences-v1-GetRobotSequenceResponse"></a>

### GetRobotSequenceResponse
Response message with the fetched robot sequence.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| sequence | [RobotSequence](#com-empyreanmed-heracles-robot_sequences-v1-RobotSequence) |  | The robot sequence with the provided ID. |






<a name="com-empyreanmed-heracles-robot_sequences-v1-ListRobotSequencesRequest"></a>

### ListRobotSequencesRequest
Request message for listing robot sequences.

No specific fields for listing all robot sequences.






<a name="com-empyreanmed-heracles-robot_sequences-v1-ListRobotSequencesResponse"></a>

### ListRobotSequencesResponse
Response message with the listed robot sequences.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| sequences | [RobotSequence](#com-empyreanmed-heracles-robot_sequences-v1-RobotSequence) | repeated | The robot sequences matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-robot_sequences-v1-UpdateRobotSequenceRequest"></a>

### UpdateRobotSequenceRequest
Request message for updating an existing robot sequence.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| sequence | [RobotSequence](#com-empyreanmed-heracles-robot_sequences-v1-RobotSequence) | optional | The robot sequence to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the robot sequence to update. |






<a name="com-empyreanmed-heracles-robot_sequences-v1-UpdateRobotSequenceResponse"></a>

### UpdateRobotSequenceResponse
Response message with the updated robot sequence.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| sequence | [RobotSequence](#com-empyreanmed-heracles-robot_sequences-v1-RobotSequence) |  | The updated robot sequence. |





 

 

 


<a name="com-empyreanmed-heracles-robot_sequences-v1-RobotSequenceService"></a>

### RobotSequenceService
Service for performing operations on robot sequences.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListRobotSequences | [ListRobotSequencesRequest](#com-empyreanmed-heracles-robot_sequences-v1-ListRobotSequencesRequest) | [ListRobotSequencesResponse](#com-empyreanmed-heracles-robot_sequences-v1-ListRobotSequencesResponse) | Lists robot sequences. |
| GetRobotSequence | [GetRobotSequenceRequest](#com-empyreanmed-heracles-robot_sequences-v1-GetRobotSequenceRequest) | [GetRobotSequenceResponse](#com-empyreanmed-heracles-robot_sequences-v1-GetRobotSequenceResponse) | Returns a single robot sequence. |
| CreateRobotSequence | [CreateRobotSequenceRequest](#com-empyreanmed-heracles-robot_sequences-v1-CreateRobotSequenceRequest) | [CreateRobotSequenceResponse](#com-empyreanmed-heracles-robot_sequences-v1-CreateRobotSequenceResponse) | Creates a new robot sequence. |
| UpdateRobotSequence | [UpdateRobotSequenceRequest](#com-empyreanmed-heracles-robot_sequences-v1-UpdateRobotSequenceRequest) | [UpdateRobotSequenceResponse](#com-empyreanmed-heracles-robot_sequences-v1-UpdateRobotSequenceResponse) | Updates an existing robot sequence. |
| DeleteRobotSequence | [DeleteRobotSequenceRequest](#com-empyreanmed-heracles-robot_sequences-v1-DeleteRobotSequenceRequest) | [DeleteRobotSequenceResponse](#com-empyreanmed-heracles-robot_sequences-v1-DeleteRobotSequenceResponse) | Deletes a robot sequence. |

 



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

