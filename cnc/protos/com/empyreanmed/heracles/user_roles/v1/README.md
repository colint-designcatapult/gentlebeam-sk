# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/user_roles/v1/user_role.proto](#com_empyreanmed_heracles_user_roles_v1_user_role-proto)
    - [UserRole](#com-empyreanmed-heracles-user_roles-v1-UserRole)
  
- [com/empyreanmed/heracles/user_roles/v1/user_role_service.proto](#com_empyreanmed_heracles_user_roles_v1_user_role_service-proto)
    - [CreateUserRoleRequest](#com-empyreanmed-heracles-user_roles-v1-CreateUserRoleRequest)
    - [CreateUserRoleResponse](#com-empyreanmed-heracles-user_roles-v1-CreateUserRoleResponse)
    - [DeleteUserRoleRequest](#com-empyreanmed-heracles-user_roles-v1-DeleteUserRoleRequest)
    - [DeleteUserRoleResponse](#com-empyreanmed-heracles-user_roles-v1-DeleteUserRoleResponse)
    - [GetUserRoleRequest](#com-empyreanmed-heracles-user_roles-v1-GetUserRoleRequest)
    - [GetUserRoleResponse](#com-empyreanmed-heracles-user_roles-v1-GetUserRoleResponse)
    - [ListUserRolesRequest](#com-empyreanmed-heracles-user_roles-v1-ListUserRolesRequest)
    - [ListUserRolesResponse](#com-empyreanmed-heracles-user_roles-v1-ListUserRolesResponse)
    - [UpdateUserRoleRequest](#com-empyreanmed-heracles-user_roles-v1-UpdateUserRoleRequest)
    - [UpdateUserRoleResponse](#com-empyreanmed-heracles-user_roles-v1-UpdateUserRoleResponse)
  
    - [UserRoleService](#com-empyreanmed-heracles-user_roles-v1-UserRoleService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_user_roles_v1_user_role-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/user_roles/v1/user_role.proto



<a name="com-empyreanmed-heracles-user_roles-v1-UserRole"></a>

### UserRole
Represents a link between a user and a role in the system.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the plan. |
| user_id | [string](#string) | optional | The id of the user who has the role. |
| role_id | [int64](#int64) | optional | The ID of the role that is assigned to the user. |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Timestamp indicating when this user-role link was created. |





 

 

 

 



<a name="com_empyreanmed_heracles_user_roles_v1_user_role_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/user_roles/v1/user_role_service.proto



<a name="com-empyreanmed-heracles-user_roles-v1-CreateUserRoleRequest"></a>

### CreateUserRoleRequest
Request message for creating a new user-role link.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user_role | [UserRole](#com-empyreanmed-heracles-user_roles-v1-UserRole) | optional | The user-role record to create. |






<a name="com-empyreanmed-heracles-user_roles-v1-CreateUserRoleResponse"></a>

### CreateUserRoleResponse
Response with the newly created user-role link.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user_role | [UserRole](#com-empyreanmed-heracles-user_roles-v1-UserRole) |  | The user-role pivot record that was just created. |






<a name="com-empyreanmed-heracles-user_roles-v1-DeleteUserRoleRequest"></a>

### DeleteUserRoleRequest
Request message for deleting a user-role link.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The user ID part of the composite key. |
| role_id | [int64](#int64) | optional | The role ID part of the composite key. |






<a name="com-empyreanmed-heracles-user_roles-v1-DeleteUserRoleResponse"></a>

### DeleteUserRoleResponse
Response confirming successful deletion (empty).






<a name="com-empyreanmed-heracles-user_roles-v1-GetUserRoleRequest"></a>

### GetUserRoleRequest
Request message for retrieving a single user-role link.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The user ID part of the composite key. |






<a name="com-empyreanmed-heracles-user_roles-v1-GetUserRoleResponse"></a>

### GetUserRoleResponse
Response containing one user-role link.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user_role | [UserRole](#com-empyreanmed-heracles-user_roles-v1-UserRole) |  | The user-role pivot record matching the requested composite key. |






<a name="com-empyreanmed-heracles-user_roles-v1-ListUserRolesRequest"></a>

### ListUserRolesRequest
Request message for listing user-role links.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user_id | [string](#string) | optional | Filter results by parent&#39;s ID. |






<a name="com-empyreanmed-heracles-user_roles-v1-ListUserRolesResponse"></a>

### ListUserRolesResponse
Response containing multiple user-role links.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user_roles | [UserRole](#com-empyreanmed-heracles-user_roles-v1-UserRole) | repeated | The list of user-role pivot records. |






<a name="com-empyreanmed-heracles-user_roles-v1-UpdateUserRoleRequest"></a>

### UpdateUserRoleRequest
Request message for updating an existing user-role link.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user_role | [UserRole](#com-empyreanmed-heracles-user_roles-v1-UserRole) | optional | The user-role pivot record with updated fields. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | Field mask specifying which fields to update. |






<a name="com-empyreanmed-heracles-user_roles-v1-UpdateUserRoleResponse"></a>

### UpdateUserRoleResponse
Response containing the updated user-role link record.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user_role | [UserRole](#com-empyreanmed-heracles-user_roles-v1-UserRole) |  | The user-role pivot record after updates. |





 

 

 


<a name="com-empyreanmed-heracles-user_roles-v1-UserRoleService"></a>

### UserRoleService
UserRoleService manages the link between users and roles (many-to-many pivot).

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListUserRoles | [ListUserRolesRequest](#com-empyreanmed-heracles-user_roles-v1-ListUserRolesRequest) | [ListUserRolesResponse](#com-empyreanmed-heracles-user_roles-v1-ListUserRolesResponse) | Lists user-role links. |
| GetUserRole | [GetUserRoleRequest](#com-empyreanmed-heracles-user_roles-v1-GetUserRoleRequest) | [GetUserRoleResponse](#com-empyreanmed-heracles-user_roles-v1-GetUserRoleResponse) | Retrieves a single user-role link by user_id and role_id. |
| CreateUserRole | [CreateUserRoleRequest](#com-empyreanmed-heracles-user_roles-v1-CreateUserRoleRequest) | [CreateUserRoleResponse](#com-empyreanmed-heracles-user_roles-v1-CreateUserRoleResponse) | Creates a new user-role link. |
| UpdateUserRole | [UpdateUserRoleRequest](#com-empyreanmed-heracles-user_roles-v1-UpdateUserRoleRequest) | [UpdateUserRoleResponse](#com-empyreanmed-heracles-user_roles-v1-UpdateUserRoleResponse) | Updates an existing user-role link. |
| DeleteUserRole | [DeleteUserRoleRequest](#com-empyreanmed-heracles-user_roles-v1-DeleteUserRoleRequest) | [DeleteUserRoleResponse](#com-empyreanmed-heracles-user_roles-v1-DeleteUserRoleResponse) | Deletes a user-role link. |

 



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

