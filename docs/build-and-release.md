# 📄 FILE 5 - `docs/build-and-release.md`

# Build & Release Process

## 1. Overview

Defines a **repeatable, semi-automated build pipeline**.

---

## 2. Build Types

### Debug Build

- Local testing
- Fast iteration

---

### Release Build

- Optimized
- Signed
- Uploaded to Play Store

---

## 3. Android Build Configuration

- Package name defined
- Version code incremented per build
- Version name human-readable

---

## 4. Signing

Use:

- Keystore file (securely stored)

⚠️ Must remain consistent across releases

---

## 5. Scripted Builds

Goal:

- One command to build APK/AAB

Example approach:

- Unity CLI build script

---

## 6. Manual Release Flow

1. Run build script
2. Verify build locally
3. Upload to Play Console
4. Assign to test track

---

## 7. Release Checklist

- App launches
- No crashes
- Version updated
- Correct build uploaded

---

## 8. Future Improvements

- CI/CD pipeline
- Automated versioning
- Automated uploads
