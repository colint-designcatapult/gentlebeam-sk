DELETE FROM public.roles_permissions;
DELETE FROM public.user_role;
DELETE FROM public.role;

ALTER SEQUENCE public.role_id_seq RESTART;
ALTER SEQUENCE public.user_role_id_seq RESTART;
ALTER SEQUENCE public.roles_permissions_id_seq RESTART;


INSERT INTO public.role (role_name, description) VALUES ('Administrator', 'Administrator') returning id;
INSERT INTO public.role (role_name, description) VALUES ('RTT', 'RTT') returning id;
INSERT INTO public.role (role_name, description) VALUES ('Physicist', 'Physicist') returning id;
INSERT INTO public.role (role_name, description) VALUES ('Service', 'Service') returning id;

INSERT INTO public.user_role(user_id, role_id) VALUES (1, 1);


-- Admin:
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_USER_MANAGEMENT'::"PERMISSION") returning id;
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_PATIENTS_TREATMENT'::"PERMISSION") returning id;
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_PATIENTS_CLINICAL_DATA'::"PERMISSION") returning id;
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_QUALITY_ASSURANCE'::"PERMISSION") returning id;
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_SERVICES'::"PERMISSION") returning id;
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_SYSTEM_CALIBRATION'::"PERMISSION") returning id;
INSERT INTO public.roles_permissions (role_id, permission) VALUES (1, 'PERMISSION_SYSTEM_SETTINGS'::"PERMISSION") returning id;

-- RTT:
INSERT INTO public.roles_permissions (role_id, permission) VALUES (2, 'PERMISSION_PATIENTS_TREATMENT'::"PERMISSION") returning id;
INSERT INTO public.roles_permissions (role_id, permission) VALUES (2, 'PERMISSION_PATIENTS_CLINICAL_DATA'::"PERMISSION") returning id;
INSERT INTO public.roles_permissions (role_id, permission) VALUES (2, 'PERMISSION_QUALITY_ASSURANCE'::"PERMISSION") returning id;

-- Physicist
INSERT INTO public.roles_permissions (role_id, permission) VALUES (3, 'PERMISSION_PATIENTS_CLINICAL_DATA'::"PERMISSION") returning id;
INSERT INTO public.roles_permissions (role_id, permission) VALUES (3, 'PERMISSION_SYSTEM_CALIBRATION'::"PERMISSION") returning id;
INSERT INTO public.roles_permissions (role_id, permission) VALUES (3, 'PERMISSION_QUALITY_ASSURANCE'::"PERMISSION") returning id;

-- Service
INSERT INTO public.roles_permissions (role_id, permission) VALUES (4, 'PERMISSION_QUALITY_ASSURANCE'::"PERMISSION") returning id;
INSERT INTO public.roles_permissions (role_id, permission) VALUES (4, 'PERMISSION_SERVICES'::"PERMISSION") returning id;


--
SELECT * FROM public.user; SELECT * FROM public.role; SELECT * FROM public.user_role; SELECT * FROM public.roles_permissions;


