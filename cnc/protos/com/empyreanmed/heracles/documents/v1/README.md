# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/documents/v1/document.proto](#com_empyreanmed_heracles_documents_v1_document-proto)
    - [Document](#com-empyreanmed-heracles-documents-v1-Document)
  
- [com/empyreanmed/heracles/documents/v1/document_service.proto](#com_empyreanmed_heracles_documents_v1_document_service-proto)
    - [CreateDocumentRequest](#com-empyreanmed-heracles-documents-v1-CreateDocumentRequest)
    - [CreateDocumentResponse](#com-empyreanmed-heracles-documents-v1-CreateDocumentResponse)
    - [DeleteDocumentRequest](#com-empyreanmed-heracles-documents-v1-DeleteDocumentRequest)
    - [DeleteDocumentResponse](#com-empyreanmed-heracles-documents-v1-DeleteDocumentResponse)
    - [GetDocumentRequest](#com-empyreanmed-heracles-documents-v1-GetDocumentRequest)
    - [GetDocumentResponse](#com-empyreanmed-heracles-documents-v1-GetDocumentResponse)
    - [ListDocumentsRequest](#com-empyreanmed-heracles-documents-v1-ListDocumentsRequest)
    - [ListDocumentsResponse](#com-empyreanmed-heracles-documents-v1-ListDocumentsResponse)
    - [UpdateDocumentRequest](#com-empyreanmed-heracles-documents-v1-UpdateDocumentRequest)
    - [UpdateDocumentResponse](#com-empyreanmed-heracles-documents-v1-UpdateDocumentResponse)
  
    - [DocumentsService](#com-empyreanmed-heracles-documents-v1-DocumentsService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_documents_v1_document-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/documents/v1/document.proto



<a name="com-empyreanmed-heracles-documents-v1-Document"></a>

### Document
Represents a document associated with a patient.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Document id, globally unique |
| patient_id | [int64](#int64) | optional | Patient id associated with the document |
| visit_id | [int64](#int64) | optional | Visit id associated with the document |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Document creation date |
| title | [string](#string) | optional | Title of the document |
| description | [string](#string) | optional | Description of the document |
| document_url | [string](#string) | optional | URL or file path of the document |





 

 

 

 



<a name="com_empyreanmed_heracles_documents_v1_document_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/documents/v1/document_service.proto



<a name="com-empyreanmed-heracles-documents-v1-CreateDocumentRequest"></a>

### CreateDocumentRequest
Request message for creating a new document.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| document | [Document](#com-empyreanmed-heracles-documents-v1-Document) | optional | The ID of the patient for which to create the document. |






<a name="com-empyreanmed-heracles-documents-v1-CreateDocumentResponse"></a>

### CreateDocumentResponse
Response message with the created document.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| document | [Document](#com-empyreanmed-heracles-documents-v1-Document) |  | The document that was created. |






<a name="com-empyreanmed-heracles-documents-v1-DeleteDocumentRequest"></a>

### DeleteDocumentRequest
Request message for deleting a document.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| document_id | [int64](#int64) | optional | The ID of the document to delete. |






<a name="com-empyreanmed-heracles-documents-v1-DeleteDocumentResponse"></a>

### DeleteDocumentResponse
An empty response message for `DeleteDocument`.






<a name="com-empyreanmed-heracles-documents-v1-GetDocumentRequest"></a>

### GetDocumentRequest
Request message for fetching a single document.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| document_id | [int64](#int64) | optional | The ID of the document to fetch. |






<a name="com-empyreanmed-heracles-documents-v1-GetDocumentResponse"></a>

### GetDocumentResponse
Response message with the fetched document.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| document | [Document](#com-empyreanmed-heracles-documents-v1-Document) |  | The document with the provided ID. |






<a name="com-empyreanmed-heracles-documents-v1-ListDocumentsRequest"></a>

### ListDocumentsRequest
Request message for listing documents.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| patient_id | [int64](#int64) | optional | The ID of the patient for which to list documents. |






<a name="com-empyreanmed-heracles-documents-v1-ListDocumentsResponse"></a>

### ListDocumentsResponse
Response message with the listed documents.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| documents | [Document](#com-empyreanmed-heracles-documents-v1-Document) | repeated | The documents matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-documents-v1-UpdateDocumentRequest"></a>

### UpdateDocumentRequest
Request message for updating an existing document.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| document | [Document](#com-empyreanmed-heracles-documents-v1-Document) | optional | The document to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the document to update. |






<a name="com-empyreanmed-heracles-documents-v1-UpdateDocumentResponse"></a>

### UpdateDocumentResponse
Response message with the updated document.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| document | [Document](#com-empyreanmed-heracles-documents-v1-Document) |  | The updated document. |





 

 

 


<a name="com-empyreanmed-heracles-documents-v1-DocumentsService"></a>

### DocumentsService
Performs operations on documents.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListDocuments | [ListDocumentsRequest](#com-empyreanmed-heracles-documents-v1-ListDocumentsRequest) | [ListDocumentsResponse](#com-empyreanmed-heracles-documents-v1-ListDocumentsResponse) | Lists documents for a given patient. |
| GetDocument | [GetDocumentRequest](#com-empyreanmed-heracles-documents-v1-GetDocumentRequest) | [GetDocumentResponse](#com-empyreanmed-heracles-documents-v1-GetDocumentResponse) | Returns a single document. |
| CreateDocument | [CreateDocumentRequest](#com-empyreanmed-heracles-documents-v1-CreateDocumentRequest) | [CreateDocumentResponse](#com-empyreanmed-heracles-documents-v1-CreateDocumentResponse) | Creates a new document for a patient. |
| UpdateDocument | [UpdateDocumentRequest](#com-empyreanmed-heracles-documents-v1-UpdateDocumentRequest) | [UpdateDocumentResponse](#com-empyreanmed-heracles-documents-v1-UpdateDocumentResponse) | Updates an existing document. |
| DeleteDocument | [DeleteDocumentRequest](#com-empyreanmed-heracles-documents-v1-DeleteDocumentRequest) | [DeleteDocumentResponse](#com-empyreanmed-heracles-documents-v1-DeleteDocumentResponse) | Deletes a document. |

 



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

