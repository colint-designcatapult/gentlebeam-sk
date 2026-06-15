# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/users/v1/user.proto](#com_empyreanmed_heracles_users_v1_user-proto)
    - [User](#com-empyreanmed-heracles-users-v1-User)
  
- [com/empyreanmed/heracles/users/v1/user_service.proto](#com_empyreanmed_heracles_users_v1_user_service-proto)
    - [CreateUserRequest](#com-empyreanmed-heracles-users-v1-CreateUserRequest)
    - [CreateUserResponse](#com-empyreanmed-heracles-users-v1-CreateUserResponse)
    - [DeleteUserRequest](#com-empyreanmed-heracles-users-v1-DeleteUserRequest)
    - [DeleteUserResponse](#com-empyreanmed-heracles-users-v1-DeleteUserResponse)
    - [GetUserRequest](#com-empyreanmed-heracles-users-v1-GetUserRequest)
    - [GetUserResponse](#com-empyreanmed-heracles-users-v1-GetUserResponse)
    - [ListUsersRequest](#com-empyreanmed-heracles-users-v1-ListUsersRequest)
    - [ListUsersResponse](#com-empyreanmed-heracles-users-v1-ListUsersResponse)
    - [UpdateUserRequest](#com-empyreanmed-heracles-users-v1-UpdateUserRequest)
    - [UpdateUserResponse](#com-empyreanmed-heracles-users-v1-UpdateUserResponse)
  
    - [UsersService](#com-empyreanmed-heracles-users-v1-UsersService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_users_v1_user-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/users/v1/user.proto



<a name="com-empyreanmed-heracles-users-v1-User"></a>

### User
Represents a user of the system.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | User id, globally unique |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | User creation date |
| picture | [string](#string) | optional | Picture URL of the user |
| first_name | [string](#string) | optional | First name of the user |
| middle_name | [string](#string) | optional | Middle name of the user |
| last_name | [string](#string) | optional | Last name of the user |
| username | [string](#string) | optional | Username of the user |
| password | [string](#string) | optional | Encrypted password of the user |
| role | [string](#string) | optional | Role of the user within the system |
| email_address | [string](#string) | optional | Email address of the user |
| last_accessed | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Last accessed timestamp |





 

 

 

 



<a name="com_empyreanmed_heracles_users_v1_user_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/users/v1/user_service.proto



<a name="com-empyreanmed-heracles-users-v1-CreateUserRequest"></a>

### CreateUserRequest
Request message for creating a new user.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user | [User](#com-empyreanmed-heracles-users-v1-User) | optional | Details of the user to create. |






<a name="com-empyreanmed-heracles-users-v1-CreateUserResponse"></a>

### CreateUserResponse
Response message with the created user.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user | [User](#com-empyreanmed-heracles-users-v1-User) |  | The user that was created. |






<a name="com-empyreanmed-heracles-users-v1-DeleteUserRequest"></a>

### DeleteUserRequest
Request message for deleting a user.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user_id | [int64](#int64) | optional | The ID of the user to delete. |






<a name="com-empyreanmed-heracles-users-v1-DeleteUserResponse"></a>

### DeleteUserResponse
An empty response message for `DeleteUser`.






<a name="com-empyreanmed-heracles-users-v1-GetUserRequest"></a>

### GetUserRequest
Request message for fetching a single user.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user_id | [int64](#int64) | optional | The ID of the user to fetch. |






<a name="com-empyreanmed-heracles-users-v1-GetUserResponse"></a>

### GetUserResponse
Response message with the fetched user.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user | [User](#com-empyreanmed-heracles-users-v1-User) |  | The user with the provided ID. |






<a name="com-empyreanmed-heracles-users-v1-ListUsersRequest"></a>

### ListUsersRequest
Request message for listing users.






<a name="com-empyreanmed-heracles-users-v1-ListUsersResponse"></a>

### ListUsersResponse
Response message with the listed users.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| users | [User](#com-empyreanmed-heracles-users-v1-User) | repeated | The users matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-users-v1-UpdateUserRequest"></a>

### UpdateUserRequest
Request message for updating an existing user.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user | [User](#com-empyreanmed-heracles-users-v1-User) | optional | The user to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the user to update. |






<a name="com-empyreanmed-heracles-users-v1-UpdateUserResponse"></a>

### UpdateUserResponse
Response message with the updated user.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| user | [User](#com-empyreanmed-heracles-users-v1-User) |  | The updated user. |





 

 

 


<a name="com-empyreanmed-heracles-users-v1-UsersService"></a>

### UsersService
Performs operations on users.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListUsers | [ListUsersRequest](#com-empyreanmed-heracles-users-v1-ListUsersRequest) | [ListUsersResponse](#com-empyreanmed-heracles-users-v1-ListUsersResponse) | Lists users. |
| GetUser | [GetUserRequest](#com-empyreanmed-heracles-users-v1-GetUserRequest) | [GetUserResponse](#com-empyreanmed-heracles-users-v1-GetUserResponse) | Returns a single user. |
| CreateUser | [CreateUserRequest](#com-empyreanmed-heracles-users-v1-CreateUserRequest) | [CreateUserResponse](#com-empyreanmed-heracles-users-v1-CreateUserResponse) | Creates a new user. |
| UpdateUser | [UpdateUserRequest](#com-empyreanmed-heracles-users-v1-UpdateUserRequest) | [UpdateUserResponse](#com-empyreanmed-heracles-users-v1-UpdateUserResponse) | Updates an existing user. |
| DeleteUser | [DeleteUserRequest](#com-empyreanmed-heracles-users-v1-DeleteUserRequest) | [DeleteUserResponse](#com-empyreanmed-heracles-users-v1-DeleteUserResponse) | Deletes a user. |

 



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

