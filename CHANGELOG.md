# Changelog

## Unreleased

### Breaking changes

- Retired platforms removed: MPNS (Windows Phone), GCM and legacy FCM. Use FCM V1. `NotificationPlatform.Fcm` is replaced by the obsolete `NotificationPlatform.Gcm`, kept with `Mpns` for reading stored installations.
- Targets `netstandard2.0` and `net10.0`; `netstandard2.1` and `net6.0` are removed.
- Exceptions are no longer `[Serializable]`.
- `PublicKeyConstants` is removed.
- Newtonsoft.Json is removed. `Installation`, `InstallationTemplate`, `WnsSecondaryTile` and `PartialUpdateOperation` carry System.Text.Json attributes instead of Newtonsoft ones.
- `DataContractSerializer` is no longer used. `AuthorizationRules.Serializer` is removed, and `EntityDescription.ExtensionData` and `NotificationDetails.ExtensionData` are no longer populated.
- Reading XML no longer fails when a required element is missing.

### Other changes

- Listing registrations skips entries of retired platforms instead of throwing.
- No trim or AOT analyzer warnings on net10.0.
