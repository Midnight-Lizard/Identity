{{/* vim: set filetype=mustache: */}}
{{/*
Expand the name of the chart.
*/}}
{{- define "name" -}}
{{- default .Release.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Expand the name of the chart.
*/}}
{{- define "checksum" -}}
{{ $path := .[0].Template.BasePath "/" .[1] ".yaml" }}
checksum/{{- .[1] -}}: {{- include (print $.) $path | sha256sum -}}
{{- end -}}
