# The specification can be found in PS3.18 6.6. #
- [6.6 STOW-RS Request/Response](http://dicom.nema.org/dicom/2013/output/chtml/part18/sect_6.6.html)
- [6.2 Value Representation (VR)](http://dicom.nema.org/dicom/2013/output/chtml/part05/sect_6.2.html)

# STOW-RS Request #

To upload imaging study to the Dataentry portal, you need to generate HTTP request with MIME `multipart/related` Content-type with `type` part `type=application/dicom+json`. Example: `COntent-Type=multipart/related; type=application/dicom+json; boundary={MessageBoundary}`

Multipart body MUST contain both metadata and bulk data. *NOTE* The multipart request body contains all the metadata and bulk data to be stored. If the number of bulk data parts does not correspond to the number of unique BulkDataURIs in the metadata then the entire message is invalid and will generate an error status line.

The first part in the multipart request (Metadata part) will contain the following HTTP headers:
  
  - Content-Type: application/dicom+json;
  
Subsequent items will contain the following HTTP headers (depends on bulk data type)
  
  - for DICOM file(s)
    - Content-Type: application/dicom;
    - Content-Location: {BulkDataURI}
  - for JPEG file(s)
    - Content-Type: image/jpeg; 
    - Content-Location: {BulkDataURI}
    
## Examples of Metadata ##

### DICOM files ###

Table with explanations of metadata parts:

| Dicom tag | Description | Type | Required | Notice |
|---|---|---|---|---|
| 00100020 | Patient ID | LO | YES | fill with the value of Patient ID from DataEntry portal |
| 00200010 | Study ID | SH | YES | fill with the value of Study ID from DataEntry portal |
| 7FE00010 | BulkDataURI | OW | YES | BulkDataURI have to be valid Uri, unique for each dicom file and must be consistent with BulkData's Content-Location Header (see [6.6 STOW-RS Request/Response](http://dicom.nema.org/dicom/2013/output/chtml/part18/sect_6.6.html)) |

#### Multiple dicoms metadata (json) ####

```json
[{
		"00100020": {
			"vr": "LO",
			"Value": ["63afc4da-1764-4ca7-b439-400c41281625"],
		},
		"00200010": {
			"vr": "SH",
			"Value": ["e67d320b-acb0-4350-9ee9-1a479396d5f9"],
		},
		"7FE00010": {
			"vr": "OW",
			"BulkDataURI": "urn:uuid:7673868d231e490d9c4f19288e7e668d"
		}
	}, {
		"00100020": {
			"vr": "LO",
			"Value": ["63afc4da-1764-4ca7-b439-400c41281626"],
		},
		"00200010": {
			"vr": "SH",
			"Value": ["e67d320b-acb0-4350-9ee9-1a479396d5f0"],
		},
		"7FE00010": {
			"vr": "OW",
			"BulkDataURI": "urn:uuid:7673868d231e490d9c4f19288e7e668c"
		}
	}
]
```

#### Single dicom metadata (json) ####

```json
{
		"00100020": {
		"vr": "LO",
		"Value": ["63afc4da-1764-4ca7-b439-400c41281625"],
	},
  "00200010": {
		"vr": "SH",
		"Value": ["e67d320b-acb0-4350-9ee9-1a479396d5f9"],
	},
	"7FE00010": {
		"vr": "OW",
		"BulkDataURI": "urn:uuid:7673868d231e490d9c4f19288e7e668d"
	}
}
```

### JPEG files ###

Table with explanations of metadata parts:

| Dicom tag | Description | Type | Required | Description |
|---|---|---|---|---|
| 00100020 | Patient ID | LO | YES | fill with the value of Patient ID from DataEntry portal |
| 00200010 | Study ID | SH | YES | fill with the value of Study ID from DataEntry portal |
| 00080020 | Study Date | DA | YES | date of imaging study in YYYYMMDD format |
| 00080060 | Modality | CS | YES |  Modality |
| 7FE00010 | BulkDataURI | OW | YES | BulkDataURI have to be valid Uri, unique for each dicom file and must be consistent with BulkData's Content-Location Header (see [6.6 STOW-RS Request/Response](http://dicom.nema.org/dicom/2013/output/chtml/part18/sect_6.6.html)) |

#### Single jpeg metadata (json) ####

```json
{
		"00100020": {
		"vr": "LO",
		"Value": ["63afc4da-1764-4ca7-b439-400c41281625"],
	},
  "00200010": {
		"vr": "SH",
		"Value": ["e67d320b-acb0-4350-9ee9-1a479396d5f9"],
	},
	"00080020": {
		"vr": "DA",
		"Value": ["20170101"],
	},
	"00080060": {
		"vr": "CS",
		"Value": ["CR"],
	},
	"7FE00010": {
		"vr": "OW",
		"BulkDataURI": "urn:uuid:7673868d231e490d9c4f19288e7e668d"
	}
}
```
