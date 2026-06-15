# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/simulations/v1/simulation.proto](#com_empyreanmed_heracles_simulations_v1_simulation-proto)
    - [Simulation](#com-empyreanmed-heracles-simulations-v1-Simulation)
  
- [com/empyreanmed/heracles/simulations/v1/simulation_service.proto](#com_empyreanmed_heracles_simulations_v1_simulation_service-proto)
    - [CreateSimulationRequest](#com-empyreanmed-heracles-simulations-v1-CreateSimulationRequest)
    - [CreateSimulationResponse](#com-empyreanmed-heracles-simulations-v1-CreateSimulationResponse)
    - [DeleteSimulationRequest](#com-empyreanmed-heracles-simulations-v1-DeleteSimulationRequest)
    - [DeleteSimulationResponse](#com-empyreanmed-heracles-simulations-v1-DeleteSimulationResponse)
    - [GetSimulationRequest](#com-empyreanmed-heracles-simulations-v1-GetSimulationRequest)
    - [GetSimulationResponse](#com-empyreanmed-heracles-simulations-v1-GetSimulationResponse)
    - [ListSimulationsRequest](#com-empyreanmed-heracles-simulations-v1-ListSimulationsRequest)
    - [ListSimulationsResponse](#com-empyreanmed-heracles-simulations-v1-ListSimulationsResponse)
    - [UpdateSimulationRequest](#com-empyreanmed-heracles-simulations-v1-UpdateSimulationRequest)
    - [UpdateSimulationResponse](#com-empyreanmed-heracles-simulations-v1-UpdateSimulationResponse)
  
    - [SimulationService](#com-empyreanmed-heracles-simulations-v1-SimulationService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_simulations_v1_simulation-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/simulations/v1/simulation.proto



<a name="com-empyreanmed-heracles-simulations-v1-Simulation"></a>

### Simulation
Simulation represents a simulated treatment session for a specific diagnosis and visit.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Simulation id, globally unique |
| diagnosis_id | [int64](#int64) | optional | The associated diagnosis id |
| visit_id | [int64](#int64) | optional | The associated visit id |
| performed_by | [string](#string) | optional | The email of the user who performed the simulation |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Simulation creation date |
| lesion_size_l | [double](#double) | optional | Lesion size in length (L) in millimeters |
| lesion_size_w | [double](#double) | optional | Lesion size in width (W) in millimeters |
| lesion_depth | [double](#double) | optional | Depth of the lesion in millimeters |
| margin_size_l | [double](#double) | optional | Margin size l |
| margin_size_w | [double](#double) | optional | Margin size w |
| shield_size_l | [double](#double) | optional | Shield Size l |
| shield_size_w | [double](#double) | optional | Shield Size l |
| target_type | [com.empyreanmed.heracles.enums.v1.TARGETTYPE](#com-empyreanmed-heracles-enums-v1-TARGETTYPE) | optional | Target type simulation |
| setup_note | [string](#string) | optional | Setup notes for the simulation |
| status | [com.empyreanmed.heracles.enums.v1.STATUS](#com-empyreanmed-heracles-enums-v1-STATUS) | optional | Status of the simulation |





 

 

 

 



<a name="com_empyreanmed_heracles_simulations_v1_simulation_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/simulations/v1/simulation_service.proto



<a name="com-empyreanmed-heracles-simulations-v1-CreateSimulationRequest"></a>

### CreateSimulationRequest
Request message for creating a new simulation.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| simulation | [Simulation](#com-empyreanmed-heracles-simulations-v1-Simulation) | optional | Details of the simulation to create. |






<a name="com-empyreanmed-heracles-simulations-v1-CreateSimulationResponse"></a>

### CreateSimulationResponse
Response message with the created simulation.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| simulation | [Simulation](#com-empyreanmed-heracles-simulations-v1-Simulation) |  | The simulation that was created. |






<a name="com-empyreanmed-heracles-simulations-v1-DeleteSimulationRequest"></a>

### DeleteSimulationRequest
Request message for deleting a simulation.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| simulation_id | [int64](#int64) | optional | The ID of the simulation to delete. |






<a name="com-empyreanmed-heracles-simulations-v1-DeleteSimulationResponse"></a>

### DeleteSimulationResponse
An empty response message for `DeleteSimulation`.






<a name="com-empyreanmed-heracles-simulations-v1-GetSimulationRequest"></a>

### GetSimulationRequest
Request message for fetching a single simulation.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| simulation_id | [int64](#int64) | optional | The ID of the simulation to fetch. |






<a name="com-empyreanmed-heracles-simulations-v1-GetSimulationResponse"></a>

### GetSimulationResponse
Response message with the fetched simulation.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| simulation | [Simulation](#com-empyreanmed-heracles-simulations-v1-Simulation) |  | The simulation with the provided ID. |






<a name="com-empyreanmed-heracles-simulations-v1-ListSimulationsRequest"></a>

### ListSimulationsRequest
Request message for listing simulations.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| diagnosis_id | [int64](#int64) | optional | The ID of the diagnosis for which to list simulations. |






<a name="com-empyreanmed-heracles-simulations-v1-ListSimulationsResponse"></a>

### ListSimulationsResponse
Response message with the listed simulations.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| simulations | [Simulation](#com-empyreanmed-heracles-simulations-v1-Simulation) | repeated | The simulations matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-simulations-v1-UpdateSimulationRequest"></a>

### UpdateSimulationRequest
Request message for updating an existing simulation.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| simulation | [Simulation](#com-empyreanmed-heracles-simulations-v1-Simulation) | optional | The simulation to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the simulation to update. |






<a name="com-empyreanmed-heracles-simulations-v1-UpdateSimulationResponse"></a>

### UpdateSimulationResponse
Response message with the updated simulation.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| simulation | [Simulation](#com-empyreanmed-heracles-simulations-v1-Simulation) | optional | The updated simulation. |





 

 

 


<a name="com-empyreanmed-heracles-simulations-v1-SimulationService"></a>

### SimulationService
Performs operations on simulations.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListSimulations | [ListSimulationsRequest](#com-empyreanmed-heracles-simulations-v1-ListSimulationsRequest) | [ListSimulationsResponse](#com-empyreanmed-heracles-simulations-v1-ListSimulationsResponse) | Lists simulations for a given diagnosis. |
| GetSimulation | [GetSimulationRequest](#com-empyreanmed-heracles-simulations-v1-GetSimulationRequest) | [GetSimulationResponse](#com-empyreanmed-heracles-simulations-v1-GetSimulationResponse) | Returns a single simulation. |
| CreateSimulation | [CreateSimulationRequest](#com-empyreanmed-heracles-simulations-v1-CreateSimulationRequest) | [CreateSimulationResponse](#com-empyreanmed-heracles-simulations-v1-CreateSimulationResponse) | Creates a new simulation for a diagnosis. |
| UpdateSimulation | [UpdateSimulationRequest](#com-empyreanmed-heracles-simulations-v1-UpdateSimulationRequest) | [UpdateSimulationResponse](#com-empyreanmed-heracles-simulations-v1-UpdateSimulationResponse) | Updates an existing simulation. |
| DeleteSimulation | [DeleteSimulationRequest](#com-empyreanmed-heracles-simulations-v1-DeleteSimulationRequest) | [DeleteSimulationResponse](#com-empyreanmed-heracles-simulations-v1-DeleteSimulationResponse) | Deletes a simulation. |

 



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

