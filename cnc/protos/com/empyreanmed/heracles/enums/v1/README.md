# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/enums/v1/enums.proto](#com_empyreanmed_heracles_enums_v1_enums-proto)
    - [CELLTYPE](#com-empyreanmed-heracles-enums-v1-CELLTYPE)
    - [DESCRIPTION](#com-empyreanmed-heracles-enums-v1-DESCRIPTION)
    - [DEVICETYPE](#com-empyreanmed-heracles-enums-v1-DEVICETYPE)
    - [ENERGY](#com-empyreanmed-heracles-enums-v1-ENERGY)
    - [FIELDNAME](#com-empyreanmed-heracles-enums-v1-FIELDNAME)
    - [ICDCODE](#com-empyreanmed-heracles-enums-v1-ICDCODE)
    - [IMAGETYPE](#com-empyreanmed-heracles-enums-v1-IMAGETYPE)
    - [LOGTYPE](#com-empyreanmed-heracles-enums-v1-LOGTYPE)
    - [MAGNETOMETERTYPE](#com-empyreanmed-heracles-enums-v1-MAGNETOMETERTYPE)
    - [PATHOLOGY](#com-empyreanmed-heracles-enums-v1-PATHOLOGY)
    - [PATIENTIDTYPE](#com-empyreanmed-heracles-enums-v1-PATIENTIDTYPE)
    - [PATIENTSTATUS](#com-empyreanmed-heracles-enums-v1-PATIENTSTATUS)
    - [PERMISSION](#com-empyreanmed-heracles-enums-v1-PERMISSION)
    - [PHOTOTYPE](#com-empyreanmed-heracles-enums-v1-PHOTOTYPE)
    - [POSITION](#com-empyreanmed-heracles-enums-v1-POSITION)
    - [SEVERITY](#com-empyreanmed-heracles-enums-v1-SEVERITY)
    - [SEXTYPE](#com-empyreanmed-heracles-enums-v1-SEXTYPE)
    - [SITELOCATION](#com-empyreanmed-heracles-enums-v1-SITELOCATION)
    - [SSDTYPE](#com-empyreanmed-heracles-enums-v1-SSDTYPE)
    - [STATUS](#com-empyreanmed-heracles-enums-v1-STATUS)
    - [TARGETTYPE](#com-empyreanmed-heracles-enums-v1-TARGETTYPE)
    - [TDF](#com-empyreanmed-heracles-enums-v1-TDF)
    - [TEMPLATETYPE](#com-empyreanmed-heracles-enums-v1-TEMPLATETYPE)
    - [TREATMENTLOADINGSTATE](#com-empyreanmed-heracles-enums-v1-TREATMENTLOADINGSTATE)
    - [VISITTYPE](#com-empyreanmed-heracles-enums-v1-VISITTYPE)
    - [WARMUPTYPE](#com-empyreanmed-heracles-enums-v1-WARMUPTYPE)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_enums_v1_enums-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/enums/v1/enums.proto


 


<a name="com-empyreanmed-heracles-enums-v1-CELLTYPE"></a>

### CELLTYPE
CELLTYPE is an enum for cell types.

| Name | Number | Description |
| ---- | ------ | ----------- |
| CELLTYPE_UNSPECIFIED | 0 | Unspecified cell type. |
| CELLTYPE_ABERRANT | 1 | Aberrant cell type. |
| CELLTYPE_ADENOID | 2 | Adenoid cell type. |
| CELLTYPE_ATYPICAL_BASALOID_PROLIFERATION | 3 | Atypical Basaloid Proliferation cell type. |
| CELLTYPE_BASOSQUAMOUS_METATYPICAL | 4 | Basosquamous (Metatypical) cell type. |
| CELLTYPE_ADNEXAL_DIFFERENTIATION | 5 | Adnexal Differentiation cell type. |
| CELLTYPE_SQUAMOUS_DIFFERENTIATION | 6 | Squamous Differentiation cell type. |
| CELLTYPE_CLEAR_RING | 7 | Clear Ring cell type. |
| CELLTYPE_CYSTIC_CELL_CARCINOMA | 8 | Cystic Cell Carcinoma cell type. |
| CELLTYPE_FIBROEPITHELIOMA_OF_PINKUS | 9 | Fibroepithelioma of Pinkus cell type. |
| CELLTYPE_INFILTRATIVE | 10 | Infiltrative cell type. |
| CELLTYPE_KERATOTIC | 11 | Keratotic cell type. |
| CELLTYPE_MICRO_NODULAR | 12 | Micro Nodular cell type. |
| CELLTYPE_MIXED_PATTERN | 13 | Mixed Pattern cell type (BCC &#43; SCC). |
| CELLTYPE_MORPHOEIC_SCLEROSING_FIBROSING | 14 | Morphoeic/Sclerosing/Fibrosing cell type. |
| CELLTYPE_NODULAR_CLASSIC_BASAL_CELL | 15 | Nodular (Classic Basal-Cell) cell type. |
| CELLTYPE_NODULOCYSTIC | 16 | Nodulocystic cell type. |
| CELLTYPE_PIGMENTED | 17 | Pigmented cell type. |
| CELLTYPE_PLEOMORPHIC | 18 | Pleomorphic cell type. |
| CELLTYPE_POLYPOID | 19 | Polypoid cell type. |
| CELLTYPE_PORE_LIKE | 20 | Pore-like cell type. |
| CELLTYPE_RODENT_ULCER_JACOBI_ULCER | 21 | Rodent Ulcer (Jacobi Ulcer) cell type. |
| CELLTYPE_SUPERFICIAL_MULTICENTRIC | 22 | Superficial (Multicentric) cell type. |
| CELLTYPE_ACANTHOLYTIC | 23 | Acantholytic cell type. |
| CELLTYPE_ADENOID_PSEUDOGLANDULAR | 24 | Adenoid/Pseudoglandular cell type. |
| CELLTYPE_ATYPICAL_SQUAMOUS_PROLIFERATION | 25 | Atypical Squamous Proliferation cell type. |
| CELLTYPE_BASALOID | 26 | Basaloid cell type. |
| CELLTYPE_CLEAR_CELL | 27 | Clear-cell cell type. |
| CELLTYPE_ERYTHROPLASIA | 28 | Erythroplasia cell type. |
| CELLTYPE_INTRAEPIDERMAL | 29 | Intraepidermal cell type. |
| CELLTYPE_INVASIVE | 30 | Invasive cell type. |
| CELLTYPE_KERATOACANTHOMA | 31 | Keratoacanthoma cell type. |
| CELLTYPE_LARGE_CELL_KERATINIZING | 32 | Large Cell Keratinizing cell type. |
| CELLTYPE_LARGE_CELL_NON_KERATINIZING | 33 | Large Cell Non-Keratinizing cell type. |
| CELLTYPE_METAPLASIA | 34 | Metaplasia cell type. |
| CELLTYPE_MODERATELY_DIFFERENTIATED | 35 | Moderately Differentiated cell type. |
| CELLTYPE_POORLY_DIFFERENTIATED | 36 | Poorly Differentiated cell type. |
| CELLTYPE_PAPILLARY_CARCINOMA | 37 | Papillary Carcinoma cell type. |
| CELLTYPE_SIGNET_RING_CELL | 38 | Signet-ring cell type. |
| CELLTYPE_SMALL_CELL_KERATINIZING | 39 | Small Cell Keratinizing cell type. |
| CELLTYPE_SUPERFICIAL | 40 | Superficial cell type. |
| CELLTYPE_SPINDLE_CELL | 41 | Spindle Cell type. |
| CELLTYPE_VERRUCOUS | 42 | Verrucous cell type. |
| CELLTYPE_WELL_DIFFERENTIATED | 43 | Well-Differentiated cell type. |
| CELLTYPE_SUPERFICIALLY_INVASIVE | 44 | Superficially Invasive cell type. |
| CELLTYPE_OTHER | 45 | Other cell type. |
| CELLTYPE_NONE | 46 | None cell type. |



<a name="com-empyreanmed-heracles-enums-v1-DESCRIPTION"></a>

### DESCRIPTION
DESCRIPTION is an enum for description types.

| Name | Number | Description |
| ---- | ------ | ----------- |
| DESCRIPTION_UNSPECIFIED | 0 | Unspecified description type. |
| DESCRIPTION_INFUNDIBULO_CYTIC | 1 | Infundibulo-cytic description type. |
| DESCRIPTION_ULCERATED_LONG_STANDING | 2 | Ulcerated long-standing description type. |
| DESCRIPTION_ADENOSQUAMOUS | 3 | Adenosquamous description type. |
| DESCRIPTION_DESMOPLASTIC_METAPLASTIC | 4 | Desmoplastic/Metaplastic description type. |
| DESCRIPTION_RECURRENT_LESION_POST_SURGERY | 5 | Recurrent Lesion post-surgery description type. |
| DESCRIPTION_LARGE_LESION | 6 | Large Lesion description type. |
| DESCRIPTION_DEEP_LESION | 7 | Deep Lesion description type. |
| DESCRIPTION_RAPID_GROWTH | 8 | Rapid growth description type. |
| DESCRIPTION_EXTENSION_INTO_HAIR_FOLLICLE | 9 | Extension into hair follicle description type. |



<a name="com-empyreanmed-heracles-enums-v1-DEVICETYPE"></a>

### DEVICETYPE
Type of medical device used in treatments.

| Name | Number | Description |
| ---- | ------ | ----------- |
| DEVICETYPE_UNSPECIFIED | 0 | Unspecified device type. |
| DEVICETYPE_CUSTOM_FABRICATION | 1 | Custom fabrication device type. |
| DEVICETYPE_INTERNAL_EYE | 2 | Internal eye device type. |
| DEVICETYPE_GAMMA_PUTTY | 3 | Gamma putty device type. |
| DEVICETYPE_EXTERNAL_EYE | 4 | External eye device type. |
| DEVICETYPE_EAR_CANAL | 5 | Ear canal device type. |
| DEVICETYPE_INTRA_NASAL | 6 | Intra-nasal device type. |
| DEVICETYPE_MASTOID | 7 | Mastoid device type. |
| DEVICETYPE_DENTAL_PACEMAKER | 8 | Dental pacemaker device type. |
| DEVICETYPE_LEAD_APRON | 9 | Lead apron device type. |
| DEVICETYPE_THYROID | 10 | Thyroid device type. |
| DEVICETYPE_LEAD_GLASSES | 11 | Lead glasses device type. |
| DEVICETYPE_HEAD_HOLDER | 12 | Head holder device type. |
| DEVICETYPE_PILLOW | 13 | Pillow device type. |
| DEVICETYPE_PACEMAKER_SHIELD | 14 | Pacemaker shield device type. |
| DEVICETYPE_PREFABRICATED_SHIELD | 15 | Prefabricated shield device type. |
| DEVICETYPE_NO_SHIELD | 16 | No-shield device type. |



<a name="com-empyreanmed-heracles-enums-v1-ENERGY"></a>

### ENERGY
Energy values

| Name | Number | Description |
| ---- | ------ | ----------- |
| ENERGY_UNSPECIFIED | 0 | Unspecified energy |
| ENERGY_50 | 1 | 50Kv |
| ENERGY_70 | 2 | 70Kv |
| ENERGY_100 | 3 | 100Kv |



<a name="com-empyreanmed-heracles-enums-v1-FIELDNAME"></a>

### FIELDNAME
Field name enum

| Name | Number | Description |
| ---- | ------ | ----------- |
| FIELDNAME_UNSPECIFIED | 0 | N/A |
| FIELDNAME_PLUS_4L2 | 1 | FW: &#43;4L2 UI: 1 |
| FIELDNAME_PLUS_4L1 | 2 | FW: &#43;4L1 UI: 2 |
| FIELDNAME_PLUS_4C | 3 | FW: &#43;4C UI: 3 |
| FIELDNAME_PLUS_4R1 | 4 | FW: &#43;4R1 UI: 4 |
| FIELDNAME_PLUS_4R2 | 5 | FW: &#43;4R2 UI: 5 |
| FIELDNAME_PLUS_3L3 | 6 | FW: &#43;3L3 UI: 6 |
| FIELDNAME_PLUS_3L2 | 7 | FW: &#43;3L2 UI: 7 |
| FIELDNAME_PLUS_3L1 | 8 | FW: &#43;3L1 UI: 8 |
| FIELDNAME_PLUS_3R1 | 9 | FW: &#43;3R1 UI: 9 |
| FIELDNAME_PLUS_3R2 | 10 | FW: &#43;3R2 UI: 10 |
| FIELDNAME_PLUS_3R3 | 11 | FW: &#43;3R3 UI: 11 |
| FIELDNAME_PLUS_2L3 | 12 | FW: &#43;2L3 UI: 12 |
| FIELDNAME_PLUS_2L2 | 13 | FW: &#43;2L2 UI: 13 |
| FIELDNAME_PLUS_2L1 | 14 | FW: &#43;2L1 UI: 14 |
| FIELDNAME_PLUS_2C | 15 | FW: &#43;2C UI: 15 |
| FIELDNAME_PLUS_2R1 | 16 | FW: &#43;2R1 UI: 16 |
| FIELDNAME_PLUS_2R2 | 17 | FW: &#43;2R2 UI: 17 |
| FIELDNAME_PLUS_2R3 | 18 | FW: &#43;2R3 UI: 18 |
| FIELDNAME_PLUS_1L4 | 19 | FW: &#43;1L4 UI: 19 |
| FIELDNAME_PLUS_1L3 | 20 | FW: &#43;1L3 UI: 20 |
| FIELDNAME_PLUS_1L2 | 21 | FW: &#43;1L2 UI: 21 |
| FIELDNAME_PLUS_1L1 | 22 | FW: &#43;1L1 UI: 22 |
| FIELDNAME_PLUS_1R1 | 23 | FW: &#43;1R1 UI: 23 |
| FIELDNAME_PLUS_1R2 | 24 | FW: &#43;1R2 UI: 24 |
| FIELDNAME_PLUS_1R3 | 25 | FW: &#43;1R3 UI: 25 |
| FIELDNAME_PLUS_1R4 | 26 | FW: &#43;1R4 UI: 26 |
| FIELDNAME_PLUS_0L4 | 27 | FW: &#43;0L4 UI: 27 |
| FIELDNAME_PLUS_0L3 | 28 | FW: &#43;0L3 UI: 28 |
| FIELDNAME_PLUS_0L2 | 29 | FW: &#43;0L2 UI: 29 |
| FIELDNAME_PLUS_0L1 | 30 | FW: &#43;0L1 UI: 30 |
| FIELDNAME_PLUS_C | 31 | FW: C UI: 31 |
| FIELDNAME_PLUS_0R1 | 32 | FW: &#43;0R1 UI: 32 |
| FIELDNAME_PLUS_0R2 | 33 | FW: &#43;0R2 UI: 33 |
| FIELDNAME_PLUS_0R3 | 34 | FW: &#43;0R3 UI: 34 |
| FIELDNAME_PLUS_0R4 | 35 | FW: &#43;0R4 UI: 35 |
| FIELDNAME_MINUS_1L4 | 36 | FW: -1L4 UI: 36 |
| FIELDNAME_MINUS_1L3 | 37 | FW: -1L3 UI: 37 |
| FIELDNAME_MINUS_1L2 | 38 | FW: -1L2 UI: 38 |
| FIELDNAME_MINUS_1L1 | 39 | FW: -1L1 UI: 39 |
| FIELDNAME_MINUS_1R1 | 40 | FW: -1R1 UI: 40 |
| FIELDNAME_MINUS_1R2 | 41 | FW: -1R2 UI: 41 |
| FIELDNAME_MINUS_1R3 | 42 | FW: -1R3 UI: 42 |
| FIELDNAME_MINUS_1R4 | 43 | FW: -1R4 UI: 43 |
| FIELDNAME_MINUS_2L3 | 44 | FW: -2L3 UI: 44 |
| FIELDNAME_MINUS_2L2 | 45 | FW: -2L2 UI: 45 |
| FIELDNAME_MINUS_2L1 | 46 | FW: -2L1 UI: 46 |
| FIELDNAME_MINUS_2C | 47 | FW: -2C UI: 47 |
| FIELDNAME_MINUS_2R1 | 48 | FW: -2R1 UI: 48 |
| FIELDNAME_MINUS_2R2 | 49 | FW: -2R2 UI: 49 |
| FIELDNAME_MINUS_2R3 | 50 | FW: -2R3 UI: 50 |
| FIELDNAME_MINUS_3L3 | 51 | FW: -3L3 UI: 51 |
| FIELDNAME_MINUS_3L2 | 52 | FW: -3L2 UI: 52 |
| FIELDNAME_MINUS_3L1 | 53 | FW: -3L1 UI: 53 |
| FIELDNAME_MINUS_3R1 | 54 | FW: -3R1 UI: 54 |
| FIELDNAME_MINUS_3R2 | 55 | FW: -3R2 UI: 55 |
| FIELDNAME_MINUS_3R3 | 56 | FW: -3R3 UI: 56 |
| FIELDNAME_MINUS_4L2 | 57 | FW: -4L2 UI: 57 |
| FIELDNAME_MINUS_4L1 | 58 | FW: -4L1 UI: 58 |
| FIELDNAME_MINUS_4C | 59 | FW: -4C UI: 59 |
| FIELDNAME_MINUS_4R1 | 60 | FW: -4R1 UI: 60 |
| FIELDNAME_MINUS_4R2 | 61 | FW: -4R2 UI: 61 |



<a name="com-empyreanmed-heracles-enums-v1-ICDCODE"></a>

### ICDCODE
ICDCODE is an enum for ICD codes.

| Name | Number | Description |
| ---- | ------ | ----------- |
| ICDCODE_UNSPECIFIED | 0 | Unspecified ICD code. |
| ICDCODE_BCC_BREAST | 1 | BCC Breast ICD code. |
| ICDCODE_SCC_BREAST | 2 | SCC Breast ICD code. |
| ICDCODE_SCC_IS_BREAST | 3 | SCC-IS Breast ICD code. |
| ICDCODE_BCC_LEFT_EAR | 4 | BCC Left Ear ICD code. |
| ICDCODE_SCC_LEFT_EAR | 5 | SCC Left Ear ICD code. |
| ICDCODE_SCC_IS_LEFT_EAR | 6 | SCC-IS Left Ear ICD code. |
| ICDCODE_BCC_RIGHT_EAR | 7 | BCC Right Ear ICD code. |
| ICDCODE_SCC_RIGHT_EAR | 8 | SCC Right Ear ICD code. |
| ICDCODE_SCC_IS_RIGHT_EAR | 9 | SCC-IS Right Ear ICD code. |
| ICDCODE_BCC_FACE | 10 | BCC Face ICD code. *Including BCC in other parts of the face, such as Cheek, Chin, Forehead, Jaw, Pre-Auricular and Zygoma. |
| ICDCODE_SCC_FACE | 11 | SCC Face ICD code. *Including SCC in other parts of the face, such as Cheek, Chin, Forehead, Jaw, Pre-Auricular and Zygoma. |
| ICDCODE_SCC_IS_FACE | 12 | SCC-IS Face ICD code. *Including SCC-IS in other parts of the face, such as Cheek, Chin, Forehead, Jaw, Pre-Auricular and Zygoma. |
| ICDCODE_BCC_LIP | 13 | BCC Lip ICD code. |
| ICDCODE_SCC_LIP | 14 | SCC Lip ICD code. |
| ICDCODE_SCC_IS_LIP | 15 | SCC-IS Lip ICD code. |
| ICDCODE_BCC_NECK | 16 | BCC Neck ICD code. |
| ICDCODE_SCC_NECK | 17 | SCC Neck ICD code. |
| ICDCODE_SCC_IS_NECK | 18 | SCC-IS Neck ICD code. |
| ICDCODE_BCC_NOSE | 19 | BCC Nose ICD code. *Including BCC in specific parts of the nose, such as the Ala, the bridge, the tip and root. |
| ICDCODE_SCC_NOSE | 20 | SCC Nose ICD code. *Including SCC in specific parts of the nose, such as the Ala, the bridge, the tip and root. |
| ICDCODE_SCC_IS_NOSE | 21 | SCC-IS Nose ICD code. *Including SCC-IS in specific parts of the nose, such as the Ala, the bridge, the tip and root. |
| ICDCODE_BCC_SCALP | 22 | BCC Scalp ICD code. *Same for BCC_PostAuricular. |
| ICDCODE_SCC_SCALP | 23 | SCC Scalp ICD code. *Same for SCC_PostAuricular. |
| ICDCODE_SCC_IS_SCALP | 24 | SCC-IS Scalp ICD code. *Same for SCC_IS_PostAuricular. |
| ICDCODE_BCC_POSTAURICULAR | 25 | BCC PostAuricular ICD code. *Same for BCC_Scalp. |
| ICDCODE_SCC_POSTAURICULAR | 26 | SCC PostAuricular ICD code. *Same for SCC_Scalp. |
| ICDCODE_SCC_IS_POSTAURICULAR | 27 | SCC-IS PostAuricular ICD code. *Same for SCC_IS_Scalp. |
| ICDCODE_BCC_TRUNK | 28 | BCC Trunk ICD code. *Same for BCC_Chest, BCC_Abdomen and BCC_Back. |
| ICDCODE_SCC_TRUNK | 29 | SCC Trunk ICD code. *Same for SCC_Chest, SCC_Abdomen and SCC_Back. |
| ICDCODE_SCC_IS_TRUNK | 30 | SCC-IS Trunk ICD code. *Same for SCC_IS_Chest, SCC_IS_Abdomen and SCC_IS_Back. |
| ICDCODE_BCC_CHEST | 31 | BCC Chest ICD code. *Same for BCC_Trunk, BCC_Abdomen and BCC_Back. |
| ICDCODE_SCC_CHEST | 32 | SCC Chest ICD code. *Same for SCC_Trunk, SCC_Abdomen and SCC_Back. |
| ICDCODE_SCC_IS_CHEST | 33 | SCC-IS Chest ICD code. *Same for SCC_IS_Trunk, SCC_IS_Abdomen and SCC_IS_Back. |
| ICDCODE_BCC_ABDOMEN | 34 | BCC Abdomen ICD code. *Same for BCC_Trunk, BCC_Chest and BCC_Back. |
| ICDCODE_SCC_ABDOMEN | 35 | SCC Abdomen ICD code. *Same for SCC_Trunk, SCC_Chest and SCC_Back. |
| ICDCODE_SCC_IS_ABDOMEN | 36 | SCC-IS Abdomen ICD code. *Same for BCC_Trunk, BCC_Chest and BCC_Back. |
| ICDCODE_BCC_BACK | 37 | BCC Back ICD code. *Same for BCC_Trunk, BCC_Abdomen and BCC_Chest. |
| ICDCODE_SCC_BACK | 38 | SCC Back ICD code. *Same for SCC_Trunk, SCC_Abdomen and SCC_Chest. |
| ICDCODE_SCC_IS_BACK | 39 | SCC-IS Back ICD code. *Same for SCC_IS_Trunk, SCC_IS_Abdomen and SCC_IS_Chest. |
| ICDCODE_BCC_LEFT_LOWER_LIMB | 40 | BCC Left Lower Limb ICD code. *Including BCC in the left hip. |
| ICDCODE_SCC_LEFT_LOWER_LIMB | 41 | SCC Left Lower Limb ICD code. *Including SCC in the left hip. |
| ICDCODE_SCC_IS_LEFT_LOWER_LIMB | 42 | SCC-IS Left Lower Limb ICD code. *Including SCC-IS in the left hip. |
| ICDCODE_BCC_RIGHT_LOWER_LIMB | 43 | BCC Right Lower Limb ICD code. *Including BCC in the right hip. |
| ICDCODE_SCC_RIGHT_LOWER_LIMB | 44 | SCC Right Lower Limb ICD code. *Including SCC in the right hip. |
| ICDCODE_SCC_IS_RIGHT_LOWER_LIMB | 45 | SCC-IS Right Lower Limb ICD code. *Including SCC-IS in the right hip. |
| ICDCODE_BCC_LEFT_UPPER_LIMB | 46 | BCC Left Upper Limb ICD code. *Including BCC in the left shoulder. |
| ICDCODE_SCC_LEFT_UPPER_LIMB | 47 | SCC Left Upper Limb ICD code. *Including SCC in the left shoulder. |
| ICDCODE_SCC_IS_LEFT_UPPER_LIMB | 48 | SCC-IS Left Upper Limb ICD code. *Including SCC-IS in the left shoulder. |
| ICDCODE_BCC_RIGHT_UPPER_LIMB | 49 | BCC Right Upper Limb ICD code. *Including BCC in the right shoulder. |
| ICDCODE_SCC_RIGHT_UPPER_LIMB | 50 | SCC Right Upper Limb ICD code. *Including SCC in the right shoulder. |
| ICDCODE_SCC_IS_RIGHT_UPPER_LIMB | 51 | SCC-IS Right Upper Limb ICD code. *Including SCC-IS in the right shoulder. |
| ICDCODE_BCC_RIGHT_UPPER_EYELID | 52 | BCC Right Upper Eyelid ICD code. |
| ICDCODE_SCC_RIGHT_UPPER_EYELID | 53 | SCC Right Upper Eyelid ICD code. |
| ICDCODE_SCC_IS_RIGHT_UPPER_EYELID | 54 | SCC-IS Right Upper Eyelid ICD code. |
| ICDCODE_BCC_RIGHT_LOWER_EYELID | 55 | BCC Right Lower Eyelid ICD code. *Including BCC in the right Canthus. |
| ICDCODE_SCC_RIGHT_LOWER_EYELID | 56 | SCC Right Lower Eyelid ICD code. *Including SCC in the right Canthus. |
| ICDCODE_SCC_IS_RIGHT_LOWER_EYELID | 57 | SCC-IS Right Lower Eyelid ICD code. *Including SCC-IS in the right Canthus. |
| ICDCODE_BCC_LEFT_UPPER_EYELID | 58 | BCC Left Upper Eyelid ICD code. |
| ICDCODE_SCC_LEFT_UPPER_EYELID | 59 | SCC Left Upper Eyelid ICD code. |
| ICDCODE_SCC_IS_LEFT_UPPER_EYELID | 60 | SCC-IS Left Upper Eyelid ICD code. |
| ICDCODE_BCC_LEFT_LOWER_EYELID | 61 | BCC Left Lower Eyelid ICD code. *Including BCC in the left Canthus. |
| ICDCODE_SCC_LEFT_LOWER_EYELID | 62 | SCC Left Lower Eyelid ICD code. *Including SCC in the left Canthus. |
| ICDCODE_SCC_IS_LEFT_LOWER_EYELID | 63 | SCC-IS Left Lower Eyelid ICD code. *Including SCC-IS in the left Canthus. |
| ICDCODE_BASOSQUAMOUS_BREAST | 64 | Basosquamous Breast ICD code. |
| ICDCODE_BASOSQUAMOUS_LEFT_EAR | 65 | Basosquamous Left Ear ICD code. |
| ICDCODE_BASOSQUAMOUS_RIGHT_EAR | 66 | Basosquamous Right Ear ICD code. |
| ICDCODE_BASOSQUAMOUS_FACE | 67 | Basosquamous Face ICD code. *Including Basosquamous in other parts of the face, such as Cheek, Chin, Forehead, Jaw, Pre-Auricular and Zygoma. |
| ICDCODE_BASOSQUAMOUS_LIP | 68 | Basosquamous Lip ICD code. |
| ICDCODE_BASOSQUAMOUS_NECK | 69 | Basosquamous Neck ICD code. |
| ICDCODE_BASOSQUAMOUS_NOSE | 70 | Basosquamous Nose ICD code. *Including Basosquamous in specific parts of the nose, such as the Ala, the bridge, the tip and root. |
| ICDCODE_BASOSQUAMOUS_SCALP | 71 | Basosquamous Scalp ICD code. *Same as BCC_Scalp, SCC_Scalp, SCC-IS_Scalp for location references. |
| ICDCODE_BASOSQUAMOUS_POSTAURICULAR | 72 | Basosquamous PostAuricular ICD code. *Same as BCC_PostAuricular, SCC_PostAuricular, SCC-IS_PostAuricular for location references. |
| ICDCODE_BASOSQUAMOUS_TRUNK | 73 | Basosquamous Trunk ICD code. *Same as BCC_Trunk, SCC_Trunk, SCC-IS_Trunk for location references. |
| ICDCODE_BASOSQUAMOUS_CHEST | 74 | Basosquamous Chest ICD code. *Same as BCC_Chest, SCC_Chest, SCC-IS_Chest for location references. |
| ICDCODE_BASOSQUAMOUS_ABDOMEN | 75 | Basosquamous Abdomen ICD code. *Same as BCC_Abdomen, SCC_Abdomen, SCC-IS_Abdomen for location references. |
| ICDCODE_BASOSQUAMOUS_BACK | 76 | Basosquamous Back ICD code. *Same as BCC_Back, SCC_Back, SCC-IS_Back for location references. |
| ICDCODE_BASOSQUAMOUS_LEFT_LOWER_LIMB | 77 | Basosquamous Left Lower Limb ICD code. *Including Basosquamous in the left hip. |
| ICDCODE_BASOSQUAMOUS_RIGHT_LOWER_LIMB | 78 | Basosquamous Right Lower Limb ICD code. *Including Basosquamous in the right hip. |
| ICDCODE_BASOSQUAMOUS_LEFT_UPPER_LIMB | 79 | Basosquamous Left Upper Limb ICD code. *Including Basosquamous in the left shoulder. |
| ICDCODE_BASOSQUAMOUS_RIGHT_UPPER_LIMB | 80 | Basosquamous Right Upper Limb ICD code. *Including Basosquamous in the right shoulder. |
| ICDCODE_BASOSQUAMOUS_RIGHT_UPPER_EYELID | 81 | Basosquamous Right Upper Eyelid ICD code. |
| ICDCODE_BASOSQUAMOUS_RIGHT_LOWER_EYELID | 82 | Basosquamous Right Lower Eyelid ICD code. |
| ICDCODE_BASOSQUAMOUS_LEFT_UPPER_EYELID | 83 | Basosquamous Left Upper Eyelid ICD code. |
| ICDCODE_BASOSQUAMOUS_LEFT_LOWER_EYELID | 84 | Basosquamous Left Lower Eyelid ICD code. |
| ICDCODE_NONE | 85 | No ICD code |



<a name="com-empyreanmed-heracles-enums-v1-IMAGETYPE"></a>

### IMAGETYPE
Image types

| Name | Number | Description |
| ---- | ------ | ----------- |
| IMAGETYPE_UNSPECIFIED | 0 | Unspecified image type |
| IMAGETYPE_XRAY | 1 | Xray image type |
| IMAGETYPE_PHOTOACOUSTIC | 2 | PhotoAcoustic image type |
| IMAGETYPE_PHOTOSONIC | 3 | Photosonic image type |



<a name="com-empyreanmed-heracles-enums-v1-LOGTYPE"></a>

### LOGTYPE
LOGTYPE is an enum representing various types of users and systems.

| Name | Number | Description |
| ---- | ------ | ----------- |
| LOGTYPE_UNSPECIFIED | 0 | Unspecified LOGTYPE type. |
| LOGTYPE_SYSTEM | 1 | System LOGTYPE type. |
| LOGTYPE_USER | 2 | User LOGTYPE type. |
| LOGTYPE_ERROR | 3 | Error LOGTYPE type. |
| LOGTYPE_SECURITY | 4 | Security LOGTYPE type. |



<a name="com-empyreanmed-heracles-enums-v1-MAGNETOMETERTYPE"></a>

### MAGNETOMETERTYPE
Type of magnetometer enum

| Name | Number | Description |
| ---- | ------ | ----------- |
| MAGNETOMETERTYPE_UNSPECIFIED | 0 | unspecified Type of magnetometer |
| MAGNETOMETERTYPE_BACK | 1 | back Type of magnetometer |
| MAGNETOMETERTYPE_FRONT | 2 | front Type of magnetometer |



<a name="com-empyreanmed-heracles-enums-v1-PATHOLOGY"></a>

### PATHOLOGY
PATHOLOGY is an enum representing different types of pathologies.

| Name | Number | Description |
| ---- | ------ | ----------- |
| PATHOLOGY_UNSPECIFIED | 0 | Unspecified pathology. |
| PATHOLOGY_BCC | 1 | Basal cell carcinoma (BCC). |
| PATHOLOGY_SCC | 2 | Squamous cell carcinoma (SCC). |
| PATHOLOGY_SCC_IS | 3 | Squamous cell carcinoma in situ (SCC-IS). |
| PATHOLOGY_KELOID | 4 | Keloid. |
| PATHOLOGY_BASOSQUAMOUS | 5 | BASOSQUAMOUS |



<a name="com-empyreanmed-heracles-enums-v1-PATIENTIDTYPE"></a>

### PATIENTIDTYPE
An enumeration of types of patient identification number types, generally government-issued.
The patient ID is intended to identify the patient in imaging providers like PACS servers.

| Name | Number | Description |
| ---- | ------ | ----------- |
| PATIENTIDTYPE_UNSPECIFIED | 0 | Default value |
| PATIENTIDTYPE_SSN | 1 | Social-Security number, generally applicable for permanent US residents |
| PATIENTIDTYPE_PASSPORT | 2 | Passport number |
| PATIENTIDTYPE_OTHER | 3 | Some other identification scheme |



<a name="com-empyreanmed-heracles-enums-v1-PATIENTSTATUS"></a>

### PATIENTSTATUS
PATIENTSTATUS is an enum representing an activity status of a patient

| Name | Number | Description |
| ---- | ------ | ----------- |
| PATIENTSTATUS_UNSPECIFIED | 0 | Unspecified status type. |
| PATIENTSTATUS_ACTIVE | 1 | active status type. |
| PATIENTSTATUS_INACTIVE | 2 | inactive status type. |
| PATIENTSTATUS_EXPIRED | 3 | expired status type. |



<a name="com-empyreanmed-heracles-enums-v1-PERMISSION"></a>

### PERMISSION
Permission enum.

| Name | Number | Description |
| ---- | ------ | ----------- |
| PERMISSION_UNSPECIFIED | 0 | Unspecified permission. |
| PERMISSION_PATIENTS_CLINICAL_DATA | 1 | Permission to handle patient clinical data. |
| PERMISSION_PATIENTS_TREATMENT | 2 | Permission to handle patient treatments. |
| PERMISSION_SYSTEM_CALIBRATION | 3 | Permission for system calibration. |
| PERMISSION_QUALITY_ASSURANCE | 4 | Permission for quality assurance. |
| PERMISSION_SYSTEM_SETTINGS | 5 | Permission for system settings. |
| PERMISSION_USER_MANAGEMENT | 6 | Permission for user management. |
| PERMISSION_SERVICES | 7 | Permission for services usage. |



<a name="com-empyreanmed-heracles-enums-v1-PHOTOTYPE"></a>

### PHOTOTYPE
PHOTOTYPE is an enum for photo types.

| Name | Number | Description |
| ---- | ------ | ----------- |
| PHOTOTYPE_UNSPECIFIED | 0 | Unspecified photo type. |
| PHOTOTYPE_LESION_WITH_MARGIN | 1 | Lesion with margin. |
| PHOTOTYPE_FIELD_WITH_SHIELD | 2 | Field with shield. |
| PHOTOTYPE_SIMULATION_SETUP | 3 | Simulation setup. |
| PHOTOTYPE_IDENTIFICATION | 4 | Identification. |



<a name="com-empyreanmed-heracles-enums-v1-POSITION"></a>

### POSITION
The position of a patient during a procedure or simulation.

| Name | Number | Description |
| ---- | ------ | ----------- |
| POSITION_UNSPECIFIED | 0 | Default value. |
| POSITION_PRONE | 1 | Prone position, patient lying flat on a bed face down. |
| POSITION_SUPINE | 2 | Supine position, patient lying flat on a bed face up. |
| POSITION_SITTING | 3 | Sitting position, patient sitting down in a chair. |
| POSITION_LYING_RT | 4 | Side-lying position, patient lying down on their right side. |
| POSITION_LYING_LT | 5 | Side-lying position, patient lying down on their left side. |
| POSITION_HEAD_LEFT | 6 | Head turned to the left. |
| POSITION_HEAD_RIGHT | 7 | Head turned to the right. |



<a name="com-empyreanmed-heracles-enums-v1-SEVERITY"></a>

### SEVERITY
SEVERITY is an enum representing various types of treatments.

| Name | Number | Description |
| ---- | ------ | ----------- |
| SEVERITY_UNSPECIFIED | 0 | Unspecified severity type. |
| SEVERITY_INFO | 1 | info severity type. |
| SEVERITY_WARN | 2 | warm severity type. |
| SEVERITY_ERROR | 3 | error severity type. |



<a name="com-empyreanmed-heracles-enums-v1-SEXTYPE"></a>

### SEXTYPE
A patient&#39;s biological sex.

| Name | Number | Description |
| ---- | ------ | ----------- |
| SEXTYPE_UNSPECIFIED | 0 | UNSPECIFIED value |
| SEXTYPE_MALE | 1 | Male |
| SEXTYPE_FEMALE | 2 | Female |
| SEXTYPE_INTERSEX | 3 | Intersex |



<a name="com-empyreanmed-heracles-enums-v1-SITELOCATION"></a>

### SITELOCATION
SITELOCATION is an enum representing patient&#39;s threaded site location.

| Name | Number | Description |
| ---- | ------ | ----------- |
| SITELOCATION_UNSPECIFIED | 0 | Unspecified site location. |
| SITELOCATION_BREAST | 1 | Breast. |
| SITELOCATION_LEFT_EAR | 2 | Left ear. |
| SITELOCATION_RIGHT_EAR | 3 | Right ear. |
| SITELOCATION_FOREHEAD | 4 | Forehead. |
| SITELOCATION_TEMPLE | 5 | Temple. |
| SITELOCATION_ZYGOMA | 6 | Zygoma. |
| SITELOCATION_PRE_AURICULAR | 7 | Pre-auricular. |
| SITELOCATION_CHEEK | 8 | Cheek. |
| SITELOCATION_CHIN | 9 | Chin. |
| SITELOCATION_JAW | 10 | Jaw. |
| SITELOCATION_LIP | 11 | Lip. |
| SITELOCATION_NECK | 12 | Neck. |
| SITELOCATION_NOSE | 13 | Nose. |
| SITELOCATION_SCALP | 14 | Scalp. |
| SITELOCATION_POST_AURICULAR | 15 | Post-auricular. |
| SITELOCATION_TRUNK | 16 | Trunk. |
| SITELOCATION_CHEST | 17 | Chest. |
| SITELOCATION_ABDOMEN | 18 | Abdomen. |
| SITELOCATION_BACK | 19 | Back. |
| SITELOCATION_LEFT_LOWER_LIMB | 20 | Left lower limb. |
| SITELOCATION_RIGHT_LOWER_LIMB | 21 | Right lower limb. |
| SITELOCATION_LEFT_UPPER_LIMB | 22 | Left upper limb. |
| SITELOCATION_RIGHT_UPPER_LIMB | 23 | Right upper limb. |
| SITELOCATION_RIGHT_UPPER_EYELID | 24 | Right upper eyelid. |
| SITELOCATION_RIGHT_LOWER_EYELID | 25 | Right lower eyelid. |
| SITELOCATION_LEFT_UPPER_EYELID | 26 | Left upper eyelid. |
| SITELOCATION_LEFT_LOWER_EYELID | 27 | Left lower eyelid. |



<a name="com-empyreanmed-heracles-enums-v1-SSDTYPE"></a>

### SSDTYPE
SSDTYPE is an enum representing different types of source to skin distance.

| Name | Number | Description |
| ---- | ------ | ----------- |
| SSDTYPE_UNSPECIFIED | 0 | Unspecified SSD type |
| SSDTYPE_50_MM | 1 | 50 mm SSD type |
| SSDTYPE_30_MM | 2 | 30 mm SSD type |



<a name="com-empyreanmed-heracles-enums-v1-STATUS"></a>

### STATUS
Status of a plan prescription and simulation

| Name | Number | Description |
| ---- | ------ | ----------- |
| STATUS_UNSPECIFIED | 0 | Default value |
| STATUS_PENDING_APPROVAL | 1 | Status is pending approval |
| STATUS_APPROVED | 2 | Status is approved |
| STATUS_REJECTED | 3 | Status is rejected |



<a name="com-empyreanmed-heracles-enums-v1-TARGETTYPE"></a>

### TARGETTYPE
The type of a target, corresponds to a head or applicator

| Name | Number | Description |
| ---- | ------ | ----------- |
| TARGETTYPE_UNSPECIFIED | 0 | Default value |
| TARGETTYPE_IMVB_COLLIMATOR_1MM_CELL | 1 | IMVB Collimator 1mm cell |
| TARGETTYPE_IMVB_COLLIMATOR_2MM_CELL | 2 | IMVB Collimator 2mm cell |
| TARGETTYPE_IMVB_COLLIMATOR_3MM_CELL | 3 | IMVB Collimator 3mm cell |
| TARGETTYPE_IMVB_COLLIMATOR_4MM_CELL | 4 | IMVB Collimator 4mm cell |
| TARGETTYPE_IMVB_COLLIMATOR_5MM_CELL | 5 | IMVB Collimator 5mm cell |
| TARGETTYPE_IMVB_COLLIMATOR_5CM_SSD_0POINT5CM_FIELD_05MM_CELL | 6 | IMVB Collimator 5cm SSD, 0.5cm field, 0.5mm cell |
| TARGETTYPE_IMVB_COLLIMATOR_5CM_SSD_1CM_FIELD_1MM_CELL | 7 | IMVB Collimator 5cm SSD, 1cm field, 1mm cell |
| TARGETTYPE_IMVB_COLLIMATOR_5CM_SSD_1POINT5CM_FIELD_1MM_CELL | 8 | IMVB Collimator 5cm SSD, 1.5cm field, 1mm cell |
| TARGETTYPE_IMVB_COLLIMATOR_5CM_SSD_2CM_FIELD_1POINT5MM_CELL | 9 | IMVB Collimator 5cm SSD, 2cm field, 1.5mm cell |
| TARGETTYPE_IMVB_COLLIMATOR_5CM_SSD_3CM_FIELD_1POINT5MM_CELL | 10 | IMVB Collimator 5cm SSD, 3cm field, 1.5mm cell |
| TARGETTYPE_IMVB_COLLIMATOR_6MMSPOT_LARGECENTRAL_CELL | 11 | IMVB Collimator, 6MM spot, large central cell 1.5mm cell |
| TARGETTYPE_QC_COLLIMATOR | 12 | Target Type collimator |
| TARGETTYPE_50MM_SSD_15MM_FIELD | 13 | 50mm SSD 15mm field single cell |
| TARGETTYPE_50MM_SSD_20MM_FIELD | 14 | 50mm SSD 20mm field single cell |
| TARGETTYPE_50MM_SSD_30MM_FIELD | 15 | 50mm SSD 30mm field single cell |
| TARGETTYPE_50MM_SSD_40MM_FIELD | 16 | 50mm SSD 40mm field single cell |
| TARGETTYPE_50MM_SSD_50MM_FIELD | 17 | 50mm SSD 50mm field single cell |



<a name="com-empyreanmed-heracles-enums-v1-TDF"></a>

### TDF
Enum representing different TDF values.

| Name | Number | Description |
| ---- | ------ | ----------- |
| TDF_UNSPECIFIED | 0 | Default value when TDF is not specified. |
| TDF_94 | 1 | TDF value for 94. |
| TDF_95 | 2 | TDF value for 95. |
| TDF_96 | 3 | TDF value for 96. |
| TDF_97 | 4 | TDF value for 97. |
| TDF_98 | 5 | TDF value for 98. |
| TDF_99 | 6 | TDF value for 99. |
| TDF_100 | 7 | TDF value for 100. |
| TDF_101 | 8 | TDF value for 101. |
| TDF_102 | 9 | TDF value for 102. |



<a name="com-empyreanmed-heracles-enums-v1-TEMPLATETYPE"></a>

### TEMPLATETYPE
Template Type Enum to categorize different medical templates used in treatment planning.

| Name | Number | Description |
| ---- | ------ | ----------- |
| TEMPLATETYPE_UNSPECIFIED | 0 | Default value indicating that the template type is unspecified or not set. |
| TEMPLATETYPE_SIMULATION | 1 | Template used for simulating lesions, including surrounding margins. |
| TEMPLATETYPE_TREATMENT | 2 | Template used during treatment planning, such as defining fields and shielding areas. |
| TEMPLATETYPE_FOLLOWUP | 3 | Template for follow-up visits or checkups after treatment. |
| TEMPLATETYPE_OTV | 4 | Template for On-Treatment Verification (OTV) or periodic evaluations during treatment. |
| TEMPLATETYPE_OTHER | 5 | Template for other types not covered by the specified categories. |



<a name="com-empyreanmed-heracles-enums-v1-TREATMENTLOADINGSTATE"></a>

### TREATMENTLOADINGSTATE
TREATMENTLOADINGSTATE is an enum for states of treatment loading for plan

| Name | Number | Description |
| ---- | ------ | ----------- |
| TREATMENTLOADINGSTATE_UNSPECIFIED | 0 | Unspecified treatment loading state |
| TREATMENTLOADINGSTATE_UNLOADED | 1 | Unloaded state for treatment |
| TREATMENTLOADINGSTATE_PENDINGLOAD | 2 | Pending loading state for treatment |
| TREATMENTLOADINGSTATE_LOADED | 3 | Loaded state for treatment |
| TREATMENTLOADINGSTATE_PARTIALPENDINGLOAD | 4 | A plan is &#34;sent&#34; to external for partial treatment |



<a name="com-empyreanmed-heracles-enums-v1-VISITTYPE"></a>

### VISITTYPE
VISITTYPE is an enum for visit types.

| Name | Number | Description |
| ---- | ------ | ----------- |
| VISITTYPE_UNSPECIFIED | 0 | Unspecified visit type. |
| VISITTYPE_SIMULATION | 1 | Simulation visit type. |
| VISITTYPE_TREATMENT | 2 | Treatment visit type. |
| VISITTYPE_OTV | 3 | On-treatment visit. |
| VISITTYPE_NON_ENCOUNTER_NOTES | 4 | Non-encounter notes. |
| VISITTYPE_FOLLOW_UP | 5 | Follow-up visit. |
| VISITTYPE_SKIN_CHECK | 6 | Skin check visit. |



<a name="com-empyreanmed-heracles-enums-v1-WARMUPTYPE"></a>

### WARMUPTYPE
WARMUPTYPE is an enum representing types of available warmups.

| Name | Number | Description |
| ---- | ------ | ----------- |
| WARMUPTYPE_UNSPECIFIED | 0 | Unspecified warmup type. |
| WARMUPTYPE_FAST | 1 | Fast warmup type. |
| WARMUPTYPE_FULL | 2 | Full warmup type. |


 

 

 



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

