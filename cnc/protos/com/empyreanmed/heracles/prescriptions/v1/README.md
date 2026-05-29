# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/prescriptions/v1/prescription.proto](#com_empyreanmed_heracles_prescriptions_v1_prescription-proto)
    - [Prescription](#com-empyreanmed-heracles-prescriptions-v1-Prescription)
  
- [com/empyreanmed/heracles/prescriptions/v1/prescription_service.proto](#com_empyreanmed_heracles_prescriptions_v1_prescription_service-proto)
    - [CreatePrescriptionRequest](#com-empyreanmed-heracles-prescriptions-v1-CreatePrescriptionRequest)
    - [CreatePrescriptionResponse](#com-empyreanmed-heracles-prescriptions-v1-CreatePrescriptionResponse)
    - [DeletePrescriptionRequest](#com-empyreanmed-heracles-prescriptions-v1-DeletePrescriptionRequest)
    - [DeletePrescriptionResponse](#com-empyreanmed-heracles-prescriptions-v1-DeletePrescriptionResponse)
    - [GetPrescriptionRequest](#com-empyreanmed-heracles-prescriptions-v1-GetPrescriptionRequest)
    - [GetPrescriptionResponse](#com-empyreanmed-heracles-prescriptions-v1-GetPrescriptionResponse)
    - [ListPrescriptionsRequest](#com-empyreanmed-heracles-prescriptions-v1-ListPrescriptionsRequest)
    - [ListPrescriptionsResponse](#com-empyreanmed-heracles-prescriptions-v1-ListPrescriptionsResponse)
    - [UpdatePrescriptionRequest](#com-empyreanmed-heracles-prescriptions-v1-UpdatePrescriptionRequest)
    - [UpdatePrescriptionResponse](#com-empyreanmed-heracles-prescriptions-v1-UpdatePrescriptionResponse)
  
    - [PrescriptionService](#com-empyreanmed-heracles-prescriptions-v1-PrescriptionService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_prescriptions_v1_prescription-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/prescriptions/v1/prescription.proto



<a name="com-empyreanmed-heracles-prescriptions-v1-Prescription"></a>

### Prescription
Prescription is a description of a treatment plan, containing a sequence of fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Plan id, globally unique |
| simulation_id | [int64](#int64) | optional | The associated simulation |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Plan creation date |
| txs_per_week | [int32](#int32) | optional | Number of fractions each week |
| energy | [com.empyreanmed.heracles.enums.v1.ENERGY](#com-empyreanmed-heracles-enums-v1-ENERGY) | optional | Prescribed Energy |
| dwell_time | [double](#double) | optional | prescribed dwell time |
| tdf | [com.empyreanmed.heracles.enums.v1.TDF](#com-empyreanmed-heracles-enums-v1-TDF) | optional | Time Dose Fractionation factor |
| min_tdf | [com.empyreanmed.heracles.enums.v1.TDF](#com-empyreanmed-heracles-enums-v1-TDF) | optional | Minimum Time Dose Fractination factor |
| daily_dose | [float](#float) | optional | Daily dose, or dose per fraction, in centi-gray |
| number_of_fxs | [int32](#int32) | optional | Total number of fractions |
| total_dose | [float](#float) | optional | NumberofFXs * DailyDose [cGy] |
| status | [com.empyreanmed.heracles.enums.v1.STATUS](#com-empyreanmed-heracles-enums-v1-STATUS) | optional | Status of the prescription |





 

 

 

 



<a name="com_empyreanmed_heracles_prescriptions_v1_prescription_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/prescriptions/v1/prescription_service.proto



<a name="com-empyreanmed-heracles-prescriptions-v1-CreatePrescriptionRequest"></a>

### CreatePrescriptionRequest
Request message for creating a new prescription.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| prescription | [Prescription](#com-empyreanmed-heracles-prescriptions-v1-Prescription) | optional | Details of the prescription to create. |






<a name="com-empyreanmed-heracles-prescriptions-v1-CreatePrescriptionResponse"></a>

### CreatePrescriptionResponse
Response message with the created prescription.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| prescription | [Prescription](#com-empyreanmed-heracles-prescriptions-v1-Prescription) | optional | The prescription that was created. |






<a name="com-empyreanmed-heracles-prescriptions-v1-DeletePrescriptionRequest"></a>

### DeletePrescriptionRequest
Request message for deleting a prescription.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| prescription_id | [int64](#int64) | optional | The ID of the prescription to delete. |






<a name="com-empyreanmed-heracles-prescriptions-v1-DeletePrescriptionResponse"></a>

### DeletePrescriptionResponse
An empty response message for `DeletePrescription`.






<a name="com-empyreanmed-heracles-prescriptions-v1-GetPrescriptionRequest"></a>

### GetPrescriptionRequest
Request message for fetching a single prescription.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the prescription to fetch. |






<a name="com-empyreanmed-heracles-prescriptions-v1-GetPrescriptionResponse"></a>

### GetPrescriptionResponse
Response message with the fetched prescription.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| prescription | [Prescription](#com-empyreanmed-heracles-prescriptions-v1-Prescription) |  | The prescription with the provided ID. |






<a name="com-empyreanmed-heracles-prescriptions-v1-ListPrescriptionsRequest"></a>

### ListPrescriptionsRequest
Request message for listing prescriptions.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| simulation_id | [int64](#int64) | optional | The ID of the simulation for which to list prescriptions. |






<a name="com-empyreanmed-heracles-prescriptions-v1-ListPrescriptionsResponse"></a>

### ListPrescriptionsResponse
Response message with the listed prescriptions.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| prescriptions | [Prescription](#com-empyreanmed-heracles-prescriptions-v1-Prescription) | repeated | The prescriptions matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-prescriptions-v1-UpdatePrescriptionRequest"></a>

### UpdatePrescriptionRequest
Request message for updating an existing prescription.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| prescription | [Prescription](#com-empyreanmed-heracles-prescriptions-v1-Prescription) | optional | The prescription to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the prescription to update. |






<a name="com-empyreanmed-heracles-prescriptions-v1-UpdatePrescriptionResponse"></a>

### UpdatePrescriptionResponse
Response message with the updated prescription.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| prescription | [Prescription](#com-empyreanmed-heracles-prescriptions-v1-Prescription) |  | The updated prescription. |





 

 

 


<a name="com-empyreanmed-heracles-prescriptions-v1-PrescriptionService"></a>

### PrescriptionService
Performs operations on prescriptions.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListPrescriptions | [ListPrescriptionsRequest](#com-empyreanmed-heracles-prescriptions-v1-ListPrescriptionsRequest) | [ListPrescriptionsResponse](#com-empyreanmed-heracles-prescriptions-v1-ListPrescriptionsResponse) | Lists prescriptions for a given simulation. |
| GetPrescription | [GetPrescriptionRequest](#com-empyreanmed-heracles-prescriptions-v1-GetPrescriptionRequest) | [GetPrescriptionResponse](#com-empyreanmed-heracles-prescriptions-v1-GetPrescriptionResponse) | Returns a single prescription. |
| CreatePrescription | [CreatePrescriptionRequest](#com-empyreanmed-heracles-prescriptions-v1-CreatePrescriptionRequest) | [CreatePrescriptionResponse](#com-empyreanmed-heracles-prescriptions-v1-CreatePrescriptionResponse) | Creates a new prescription for a simulation. |
| UpdatePrescription | [UpdatePrescriptionRequest](#com-empyreanmed-heracles-prescriptions-v1-UpdatePrescriptionRequest) | [UpdatePrescriptionResponse](#com-empyreanmed-heracles-prescriptions-v1-UpdatePrescriptionResponse) | Updates an existing prescription. |
| DeletePrescription | [DeletePrescriptionRequest](#com-empyreanmed-heracles-prescriptions-v1-DeletePrescriptionRequest) | [DeletePrescriptionResponse](#com-empyreanmed-heracles-prescriptions-v1-DeletePrescriptionResponse) | Deletes a prescription. |

 



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

