# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/plans/v1/plan.proto](#com_empyreanmed_heracles_plans_v1_plan-proto)
    - [Plan](#com-empyreanmed-heracles-plans-v1-Plan)
  
- [com/empyreanmed/heracles/plans/v1/plan_service.proto](#com_empyreanmed_heracles_plans_v1_plan_service-proto)
    - [CreatePlanRequest](#com-empyreanmed-heracles-plans-v1-CreatePlanRequest)
    - [CreatePlanResponse](#com-empyreanmed-heracles-plans-v1-CreatePlanResponse)
    - [DeletePlanRequest](#com-empyreanmed-heracles-plans-v1-DeletePlanRequest)
    - [DeletePlanResponse](#com-empyreanmed-heracles-plans-v1-DeletePlanResponse)
    - [FindLoadedPlanRequest](#com-empyreanmed-heracles-plans-v1-FindLoadedPlanRequest)
    - [FindLoadedPlanResponse](#com-empyreanmed-heracles-plans-v1-FindLoadedPlanResponse)
    - [FindPendingPlanRequest](#com-empyreanmed-heracles-plans-v1-FindPendingPlanRequest)
    - [FindPendingPlanResponse](#com-empyreanmed-heracles-plans-v1-FindPendingPlanResponse)
    - [GetPlanRequest](#com-empyreanmed-heracles-plans-v1-GetPlanRequest)
    - [GetPlanResponse](#com-empyreanmed-heracles-plans-v1-GetPlanResponse)
    - [ListPlansRequest](#com-empyreanmed-heracles-plans-v1-ListPlansRequest)
    - [ListPlansResponse](#com-empyreanmed-heracles-plans-v1-ListPlansResponse)
    - [LoadForTreatmentEventsRequest](#com-empyreanmed-heracles-plans-v1-LoadForTreatmentEventsRequest)
    - [LoadForTreatmentEventsResponse](#com-empyreanmed-heracles-plans-v1-LoadForTreatmentEventsResponse)
    - [LoadForTreatmentRequest](#com-empyreanmed-heracles-plans-v1-LoadForTreatmentRequest)
    - [LoadForTreatmentResponse](#com-empyreanmed-heracles-plans-v1-LoadForTreatmentResponse)
    - [PlanEventsRequest](#com-empyreanmed-heracles-plans-v1-PlanEventsRequest)
    - [PlanEventsResponse](#com-empyreanmed-heracles-plans-v1-PlanEventsResponse)
    - [TreatmentLoadAckRequest](#com-empyreanmed-heracles-plans-v1-TreatmentLoadAckRequest)
    - [TreatmentLoadAckResponse](#com-empyreanmed-heracles-plans-v1-TreatmentLoadAckResponse)
    - [UnloadFromTreatmentRequest](#com-empyreanmed-heracles-plans-v1-UnloadFromTreatmentRequest)
    - [UnloadFromTreatmentResponse](#com-empyreanmed-heracles-plans-v1-UnloadFromTreatmentResponse)
    - [UpdatePlanPrescriptionSimulationStatusRequest](#com-empyreanmed-heracles-plans-v1-UpdatePlanPrescriptionSimulationStatusRequest)
    - [UpdatePlanPrescriptionSimulationStatusResponse](#com-empyreanmed-heracles-plans-v1-UpdatePlanPrescriptionSimulationStatusResponse)
    - [UpdatePlanRequest](#com-empyreanmed-heracles-plans-v1-UpdatePlanRequest)
    - [UpdatePlanResponse](#com-empyreanmed-heracles-plans-v1-UpdatePlanResponse)
  
    - [PlanService](#com-empyreanmed-heracles-plans-v1-PlanService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_plans_v1_plan-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/plans/v1/plan.proto



<a name="com-empyreanmed-heracles-plans-v1-Plan"></a>

### Plan
Plan is a description of a treatment plan, containing a sequence of fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Plan id, globally unique |
| prescription_id | [int64](#int64) | optional | The associated prescription |
| approved_by | [string](#string) | optional | The associated user approving the Plan |
| origin_series_id | [int64](#int64) | optional | Plan&#39;s series id |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Plan creation date |
| target_type | [com.empyreanmed.heracles.enums.v1.TARGETTYPE](#com-empyreanmed-heracles-enums-v1-TARGETTYPE) | optional | The type of the head/target/applicator |
| status | [com.empyreanmed.heracles.enums.v1.STATUS](#com-empyreanmed-heracles-enums-v1-STATUS) | optional | The plan&#39;s status |
| treatment_loading_state | [com.empyreanmed.heracles.enums.v1.TREATMENTLOADINGSTATE](#com-empyreanmed-heracles-enums-v1-TREATMENTLOADINGSTATE) | optional | treatmentLoadingState Enum |
| name | [string](#string) | optional | The associated plan&#39;s name |





 

 

 

 



<a name="com_empyreanmed_heracles_plans_v1_plan_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/plans/v1/plan_service.proto



<a name="com-empyreanmed-heracles-plans-v1-CreatePlanRequest"></a>

### CreatePlanRequest
Request message for creating a new plan.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| plan | [Plan](#com-empyreanmed-heracles-plans-v1-Plan) | optional | Details of the plan to create. |






<a name="com-empyreanmed-heracles-plans-v1-CreatePlanResponse"></a>

### CreatePlanResponse
Response message with the created plan.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| plan | [Plan](#com-empyreanmed-heracles-plans-v1-Plan) |  | The plan that was created. |






<a name="com-empyreanmed-heracles-plans-v1-DeletePlanRequest"></a>

### DeletePlanRequest
Request message for deleting a plan.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| plan_id | [int64](#int64) | optional | The ID of the plan to delete. |






<a name="com-empyreanmed-heracles-plans-v1-DeletePlanResponse"></a>

### DeletePlanResponse
An empty response message for `DeletePlan`.






<a name="com-empyreanmed-heracles-plans-v1-FindLoadedPlanRequest"></a>

### FindLoadedPlanRequest
Request for finding the loaded Plan






<a name="com-empyreanmed-heracles-plans-v1-FindLoadedPlanResponse"></a>

### FindLoadedPlanResponse
Response with the loaded plan


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| plan | [Plan](#com-empyreanmed-heracles-plans-v1-Plan) |  | Loaded Plan |






<a name="com-empyreanmed-heracles-plans-v1-FindPendingPlanRequest"></a>

### FindPendingPlanRequest
Request for finding the pending Plan






<a name="com-empyreanmed-heracles-plans-v1-FindPendingPlanResponse"></a>

### FindPendingPlanResponse
Response with the pending plan


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| plan | [Plan](#com-empyreanmed-heracles-plans-v1-Plan) |  | Loaded Plan |






<a name="com-empyreanmed-heracles-plans-v1-GetPlanRequest"></a>

### GetPlanRequest
Request message for fetching a single plan.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| plan_id | [int64](#int64) | optional | The ID of the plan to fetch. |






<a name="com-empyreanmed-heracles-plans-v1-GetPlanResponse"></a>

### GetPlanResponse
Response message with the fetched plan.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| plan | [Plan](#com-empyreanmed-heracles-plans-v1-Plan) |  | The plan with the provided ID. |






<a name="com-empyreanmed-heracles-plans-v1-ListPlansRequest"></a>

### ListPlansRequest
Request message for listing plans.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| prescription_id | [int64](#int64) | optional | The ID of the prescription for which to list plans. |






<a name="com-empyreanmed-heracles-plans-v1-ListPlansResponse"></a>

### ListPlansResponse
Response message with the listed plans.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| plans | [Plan](#com-empyreanmed-heracles-plans-v1-Plan) | repeated | The plans matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-plans-v1-LoadForTreatmentEventsRequest"></a>

### LoadForTreatmentEventsRequest
Request for opening a long-running stream of events of load for treatment, optionally replaying events for Patient.






<a name="com-empyreanmed-heracles-plans-v1-LoadForTreatmentEventsResponse"></a>

### LoadForTreatmentEventsResponse
Response for an event occurrence


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| patient | [com.empyreanmed.heracles.patients.v1.Patient](#com-empyreanmed-heracles-patients-v1-Patient) |  | Patient loaded for treatment |
| plan | [Plan](#com-empyreanmed-heracles-plans-v1-Plan) |  | Plan Loaded for treatment |
| treatment_field | [com.empyreanmed.heracles.treatment_fields.v1.TreatmentField](#com-empyreanmed-heracles-treatment_fields-v1-TreatmentField) |  | Treatment Field Loaded for treatment |






<a name="com-empyreanmed-heracles-plans-v1-LoadForTreatmentRequest"></a>

### LoadForTreatmentRequest
Request for changing Plan&#39;s treatment_loading_state to TREATMENTLOADINGSTATE_PENDINGLOAD and sending an event of load for treatment.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the plan to be loaded for Treatment. |
| is_partial | [bool](#bool) | optional | Boolean flag to select between PENDINGLOAD and PARTIALPENDINGLOAD states. |






<a name="com-empyreanmed-heracles-plans-v1-LoadForTreatmentResponse"></a>

### LoadForTreatmentResponse
Response for changing Plan&#39;s treatment_loading_state and sending an event of load for treatment.






<a name="com-empyreanmed-heracles-plans-v1-PlanEventsRequest"></a>

### PlanEventsRequest
Request for opening a long-running stream of events of changes in Plan&#39;s table.






<a name="com-empyreanmed-heracles-plans-v1-PlanEventsResponse"></a>

### PlanEventsResponse
Response for an event occurrence of changes in Plan&#39;s table.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| plan | [Plan](#com-empyreanmed-heracles-plans-v1-Plan) |  | Plan changed |






<a name="com-empyreanmed-heracles-plans-v1-TreatmentLoadAckRequest"></a>

### TreatmentLoadAckRequest
Request for changing Plan&#39;s treatment_loading_state to TREATMENTLOADINGSTATE_LOADED and sending an event of load for treatment.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the plan to be loaded for Treatment. |






<a name="com-empyreanmed-heracles-plans-v1-TreatmentLoadAckResponse"></a>

### TreatmentLoadAckResponse
Response for changing Plan&#39;s treatment_loading_state to TREATMENTLOADINGSTATE_LOADED and sending an event of load for treatment.






<a name="com-empyreanmed-heracles-plans-v1-UnloadFromTreatmentRequest"></a>

### UnloadFromTreatmentRequest
Request for sending an event of load for treatment


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the plan to be loaded for Treatment. |






<a name="com-empyreanmed-heracles-plans-v1-UnloadFromTreatmentResponse"></a>

### UnloadFromTreatmentResponse
Response for an event occurrence






<a name="com-empyreanmed-heracles-plans-v1-UpdatePlanPrescriptionSimulationStatusRequest"></a>

### UpdatePlanPrescriptionSimulationStatusRequest
Request message for updating the status of a Plan, Prescription, and Simulation.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| username | [string](#string) |  | The username of the user performing the update. |
| password | [string](#string) |  | The password of the user performing the update. |
| plan_id | [int64](#int64) |  | The ID of the plan to update. |
| status | [com.empyreanmed.heracles.enums.v1.STATUS](#com-empyreanmed-heracles-enums-v1-STATUS) |  | The new status to set for the Plan, Prescription, and Simulation. |






<a name="com-empyreanmed-heracles-plans-v1-UpdatePlanPrescriptionSimulationStatusResponse"></a>

### UpdatePlanPrescriptionSimulationStatusResponse
Response message for the update of Plan, Prescription, and Simulation.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| updated_plan | [Plan](#com-empyreanmed-heracles-plans-v1-Plan) |  | The updated plan with the new status and approved_by field. |






<a name="com-empyreanmed-heracles-plans-v1-UpdatePlanRequest"></a>

### UpdatePlanRequest
Request message for updating an existing plan.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| plan | [Plan](#com-empyreanmed-heracles-plans-v1-Plan) | optional | The plan to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the plan to update. |






<a name="com-empyreanmed-heracles-plans-v1-UpdatePlanResponse"></a>

### UpdatePlanResponse
Response message with the updated plan.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| plan | [Plan](#com-empyreanmed-heracles-plans-v1-Plan) |  | The updated plan. |





 

 

 


<a name="com-empyreanmed-heracles-plans-v1-PlanService"></a>

### PlanService
Performs operations on plans.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListPlans | [ListPlansRequest](#com-empyreanmed-heracles-plans-v1-ListPlansRequest) | [ListPlansResponse](#com-empyreanmed-heracles-plans-v1-ListPlansResponse) | Lists plans for a given prescription. |
| GetPlan | [GetPlanRequest](#com-empyreanmed-heracles-plans-v1-GetPlanRequest) | [GetPlanResponse](#com-empyreanmed-heracles-plans-v1-GetPlanResponse) | Returns a single plan. |
| CreatePlan | [CreatePlanRequest](#com-empyreanmed-heracles-plans-v1-CreatePlanRequest) | [CreatePlanResponse](#com-empyreanmed-heracles-plans-v1-CreatePlanResponse) | Creates a new plan for a prescription. |
| UpdatePlan | [UpdatePlanRequest](#com-empyreanmed-heracles-plans-v1-UpdatePlanRequest) | [UpdatePlanResponse](#com-empyreanmed-heracles-plans-v1-UpdatePlanResponse) | Updates an existing plan. |
| DeletePlan | [DeletePlanRequest](#com-empyreanmed-heracles-plans-v1-DeletePlanRequest) | [DeletePlanResponse](#com-empyreanmed-heracles-plans-v1-DeletePlanResponse) | Deletes a plan. |
| LoadForTreatment | [LoadForTreatmentRequest](#com-empyreanmed-heracles-plans-v1-LoadForTreatmentRequest) | [LoadForTreatmentResponse](#com-empyreanmed-heracles-plans-v1-LoadForTreatmentResponse) | Change treatment state from TREATMENTLOADINGSTATE_UNLOADED to TREATMENTLOADINGSTATE_PENDINGLOAD. |
| TreatmentLoadAck | [TreatmentLoadAckRequest](#com-empyreanmed-heracles-plans-v1-TreatmentLoadAckRequest) | [TreatmentLoadAckResponse](#com-empyreanmed-heracles-plans-v1-TreatmentLoadAckResponse) | Change treatment state from TREATMENTLOADINGSTATE_PENDINGLOAD to TREATMENTLOADINGSTATE_LOADED. |
| UnloadFromTreatment | [UnloadFromTreatmentRequest](#com-empyreanmed-heracles-plans-v1-UnloadFromTreatmentRequest) | [UnloadFromTreatmentResponse](#com-empyreanmed-heracles-plans-v1-UnloadFromTreatmentResponse) | Change treatment state from TREATMENTLOADINGSTATE_LOADED to TREATMENTLOADINGSTATE_UNLOADED. |
| FindPendingPlan | [FindPendingPlanRequest](#com-empyreanmed-heracles-plans-v1-FindPendingPlanRequest) | [FindPendingPlanResponse](#com-empyreanmed-heracles-plans-v1-FindPendingPlanResponse) | Find the plan which it&#39;s TREATMENTLOADINGSTATE is PENDINGLOAD |
| FindLoadedPlan | [FindLoadedPlanRequest](#com-empyreanmed-heracles-plans-v1-FindLoadedPlanRequest) | [FindLoadedPlanResponse](#com-empyreanmed-heracles-plans-v1-FindLoadedPlanResponse) | Find the plan which it&#39;s TREATMENTLOADINGSTATE is LOADED |
| LoadForTreatmentEvents | [LoadForTreatmentEventsRequest](#com-empyreanmed-heracles-plans-v1-LoadForTreatmentEventsRequest) | [LoadForTreatmentEventsResponse](#com-empyreanmed-heracles-plans-v1-LoadForTreatmentEventsResponse) stream | External listen&#39;s for load for treatment events |
| PlanEvents | [PlanEventsRequest](#com-empyreanmed-heracles-plans-v1-PlanEventsRequest) | [PlanEventsResponse](#com-empyreanmed-heracles-plans-v1-PlanEventsResponse) stream | events for changes in Plans |
| UpdatePlanPrescriptionSimulationStatus | [UpdatePlanPrescriptionSimulationStatusRequest](#com-empyreanmed-heracles-plans-v1-UpdatePlanPrescriptionSimulationStatusRequest) | [UpdatePlanPrescriptionSimulationStatusResponse](#com-empyreanmed-heracles-plans-v1-UpdatePlanPrescriptionSimulationStatusResponse) | Updates the status of a Plan, its associated Prescription, and Simulation in a single transaction. |

 



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

