# Google Play Private Alpha Setup

## 1. Overview

Defines how to distribute the app privately using **email-based tester whitelisting**.

---

## 2. Requirements

- Google Play Console account
- One-time registration fee

---

## 3. App Creation

Steps:

1. Create new app
2. Fill:
   - App name
   - Default language
   - App type (Game)

---

## 4. Internal Testing Track

Use:

- **Internal Testing** (fastest)
  or
- **Closed Testing (Alpha)**

---

## 5. Tester Whitelisting

Add:

- Individual emails
  or
- Google Groups

Only listed users can install.

---

## 6. Build Upload

From Unity:

- Build **.aab (Android App Bundle)**

Upload:

- New release → upload bundle
- Add release notes
- Publish to test track

---

## 7. Installation Flow

Tester:

1. Receives invite link
2. Accepts tester access
3. Installs from Play Store

---

## 8. Policy Awareness (Important)

Even for private alpha:

- Basic app info required
- Privacy policy may be required
- Permissions must be justified

---

## 9. Iteration Flow

Loop:

1. Build
2. Upload
3. Test
4. Fix
5. Repeat

---

## 10. Versioning Strategy

Use:

- Incremental version codes
- Clear version names

Example:

```
0.1.0-alpha1
0.1.0-alpha2
```
