# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/visits/v1/visit.proto](#com_empyreanmed_heracles_visits_v1_visit-proto)
    - [Visit](#com-empyreanmed-heracles-visits-v1-Visit)
  
- [com/empyreanmed/heracles/visits/v1/visit_service.proto](#com_empyreanmed_heracles_visits_v1_visit_service-proto)
    - [CreateVisitRequest](#com-empyreanmed-heracles-visits-v1-CreateVisitRequest)
    - [CreateVisitResponse](#com-empyreanmed-heracles-visits-v1-CreateVisitResponse)
    - [DeleteVisitRequest](#com-empyreanmed-heracles-visits-v1-DeleteVisitRequest)
    - [DeleteVisitResponse](#com-empyreanmed-heracles-visits-v1-DeleteVisitResponse)
    - [GetVisitRequest](#com-empyreanmed-heracles-visits-v1-GetVisitRequest)
    - [GetVisitResponse](#com-empyreanmed-heracles-visits-v1-GetVisitResponse)
    - [ListVisitsRequest](#com-empyreanmed-heracles-visits-v1-ListVisitsRequest)
    - [ListVisitsResponse](#com-empyreanmed-heracles-visits-v1-ListVisitsResponse)
    - [UpdateVisitRequest](#com-empyreanmed-heracles-visits-v1-UpdateVisitRequest)
    - [UpdateVisitResponse](#com-empyreanmed-heracles-visits-v1-UpdateVisitResponse)
  
    - [VisitsService](#com-empyreanmed-heracles-visits-v1-VisitsService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_visits_v1_visit-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/visits/v1/visit.proto



<a name="com-empyreanmed-heracles-visits-v1-Visit"></a>

### Visit
Represents a visit of a patient.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Visit id, globally unique |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Visit creation date |
| patient_id | [int64](#int64) | optional | Patient id associated with the visit |
| type | [com.empyreanmed.heracles.enums.v1.VISITTYPE](#com-empyreanmed-heracles-enums-v1-VISITTYPE) | optional | Type of the visit |





 

 

 

 



<a name="com_empyreanmed_heracles_visits_v1_visit_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/visits/v1/visit_service.proto



<a name="com-empyreanmed-heracles-visits-v1-CreateVisitRequest"></a>

### CreateVisitRequest
Request message for creating a new visit.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| visit | [Visit](#com-empyreanmed-heracles-visits-v1-Visit) | optional | Details of the visit to create. |






<a name="com-empyreanmed-heracles-visits-v1-CreateVisitResponse"></a>

### CreateVisitResponse
Response message with the created visit.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| visit | [Visit](#com-empyreanmed-heracles-visits-v1-Visit) |  | The visit that was created. |






<a name="com-empyreanmed-heracles-visits-v1-DeleteVisitRequest"></a>

### DeleteVisitRequest
Request message for deleting a visit.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| visit_id | [int64](#int64) | optional | The ID of the visit to delete. |






<a name="com-empyreanmed-heracles-visits-v1-DeleteVisitResponse"></a>

### DeleteVisitResponse
An empty response message for `DeleteVisit`.






<a name="com-empyreanmed-heracles-visits-v1-GetVisitRequest"></a>

### GetVisitRequest
Request message for fetching a single visit.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| visit_id | [int64](#int64) | optional | The ID of the visit to fetch. |






<a name="com-empyreanmed-heracles-visits-v1-GetVisitResponse"></a>

### GetVisitResponse
Response message with the fetched visit.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| visit | [Visit](#com-empyreanmed-heracles-visits-v1-Visit) |  | The visit with the provided ID. |






<a name="com-empyreanmed-heracles-visits-v1-ListVisitsRequest"></a>

### ListVisitsRequest
Request message for listing visits.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| patient_id | [int64](#int64) | optional | The ID of the patient for which to list visits. |






<a name="com-empyreanmed-heracles-visits-v1-ListVisitsResponse"></a>

### ListVisitsResponse
Response message with the listed visits.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| visits | [Visit](#com-empyreanmed-heracles-visits-v1-Visit) | repeated | The visits matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-visits-v1-UpdateVisitRequest"></a>

### UpdateVisitRequest
Request message for updating an existing visit.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| visit | [Visit](#com-empyreanmed-heracles-visits-v1-Visit) | optional | The visit to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the visit to update. |






<a name="com-empyreanmed-heracles-visits-v1-UpdateVisitResponse"></a>

### UpdateVisitResponse
Response message with the updated visit.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| visit | [Visit](#com-empyreanmed-heracles-visits-v1-Visit) |  | The updated visit. |





 

 

 


<a name="com-empyreanmed-heracles-visits-v1-VisitsService"></a>

### VisitsService
Performs operations on visits.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListVisits | [ListVisitsRequest](#com-empyreanmed-heracles-visits-v1-ListVisitsRequest) | [ListVisitsResponse](#com-empyreanmed-heracles-visits-v1-ListVisitsResponse) | Lists visits for a given patient. |
| GetVisit | [GetVisitRequest](#com-empyreanmed-heracles-visits-v1-GetVisitRequest) | [GetVisitResponse](#com-empyreanmed-heracles-visits-v1-GetVisitResponse) | Returns a single visit. |
| CreateVisit | [CreateVisitRequest](#com-empyreanmed-heracles-visits-v1-CreateVisitRequest) | [CreateVisitResponse](#com-empyreanmed-heracles-visits-v1-CreateVisitResponse) | Creates a new visit for a patient. |
| UpdateVisit | [UpdateVisitRequest](#com-empyreanmed-heracles-visits-v1-UpdateVisitRequest) | [UpdateVisitResponse](#com-empyreanmed-heracles-visits-v1-UpdateVisitResponse) | Updates an existing visit. |
| DeleteVisit | [DeleteVisitRequest](#com-empyreanmed-heracles-visits-v1-DeleteVisitRequest) | [DeleteVisitResponse](#com-empyreanmed-heracles-visits-v1-DeleteVisitResponse) | Deletes a visit. |

 



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

