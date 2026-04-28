# Google Play Private Alpha Setup

## 1. Goal

Distribute the app to a small whitelist of testers via the Play Store **without
making it public**. Use **Internal Testing** for fastest turnaround.

---

## 2. Prerequisites

- Google Play Console developer account (one-time $25 fee).
- Identity verification completed (D-U-N-S or personal ID; can take days - start early).
- An AAB built per [build-and-release.md](build-and-release.md).
- A privacy policy URL hosted somewhere stable (GitHub Pages is acceptable).

---

## 3. Track choice

| Track             | Max testers  | Approval delay | Use when                  |
| ----------------- | ------------ | -------------- | ------------------------- |
| **Internal**      | 100          | Minutes        | **Default for v0.2**      |
| Closed (Alpha)    | Larger lists | ~hours         | When > 100 testers needed |
| Open              | Public       | Hours/days     | Not for v0.2              |

---

## 4. App creation (one-time)

Play Console → **Create app**:

- App name, default language
- App or **Game** → Game
- Free / Paid → Free
- Declarations: ads (No), content guidelines, US export laws

---

## 5. Required pre-launch forms (even for internal track)

These block AAB upload or rollout if missing:

- [ ] **App access** - describe demo flow if any login (none for v0.2 → "All functionality available without restrictions")
- [ ] **Ads** - declare No
- [ ] **Content rating** - fill IARC questionnaire
- [ ] **Target audience** - pick age group
- [ ] **News app** - No
- [ ] **COVID-19 contact tracing** - No
- [ ] **Data safety** - declare what's collected (v0.2: nothing leaves device)
- [ ] **Government app** - No
- [ ] **Financial features** - No
- [ ] **Health** - No
- [ ] **Privacy policy URL**

For v0.2 (offline, no analytics) the Data Safety form is short: *No data
collected, no data shared.* Keep this honest - adding analytics later requires
updating the form.

---

## 6. Tester whitelisting (Internal track)

1. **Testing → Internal testing → Testers tab**
2. Create an email list (or use a Google Group).
3. Add tester emails (must be Gmail / Google-account-linked addresses).
4. Save. Share the **opt-in URL** shown on the page with testers.

Testers must:

1. Click opt-in URL on the device's Google account.
2. Wait a few minutes for propagation.
3. Install via Play Store (search by package name or use the test link).

---

## 7. Build upload

1. **Internal testing → Create new release**.
2. Upload `release.aab`.
3. Release name: matches `bundleVersion` (e.g. `0.2.0-alpha3`).
4. Release notes: 1-3 lines, plain English, what changed.
5. **Save → Review release → Start rollout to Internal testing**.

First upload only:

- Play will scan the AAB and surface warnings (target SDK, deprecated APIs, permissions).
- Resolve **all** errors. Warnings: judgment call but document any waivers.

---

## 8. Iteration loop

```
edit → build release AAB → bump versionCode → upload new release → testers update from Play Store
```

Testers normally get the update within minutes. Force-refresh: open Play Store → app page → pull to refresh.

---

## 9. Versioning conventions

Match [build-and-release.md §4](build-and-release.md#4-versioning):

```
0.2.0-alpha1   versionCode 10
0.2.0-alpha2   versionCode 11
0.2.0-beta1    versionCode 20
0.2.0          versionCode 100
```

Always strictly increasing `versionCode`. Once uploaded, a versionCode cannot be reused.

---

## 10. What testers see

- Listed as **(Unreleased)** on their Play Store page - fine.
- Can leave private feedback via the opt-in page.
- If they uninstall and reinstall, position/save is **gone** (local-only persistence in v0.2).

---

## 11. Pulling a release

If a build is broken:

1. **Internal testing → Releases overview → Halt rollout** (instant).
2. Upload a new build with a higher versionCode.
3. Do **not** delete the previous AAB; it's needed for diffing.

---

## 12. Common rejection reasons

| Reason                              | Fix                                              |
| ----------------------------------- | ------------------------------------------------ |
| Target SDK < 35                     | Update in Player Settings (see build-and-release.md) |
| 32-bit-only AAB                     | Enable ARM64; disable ARMv7                      |
| Missing Data Safety form            | Fill in §5                                       |
| Privacy policy URL 404              | Host stable page                                 |
| Permissions declared but unused     | Trim from manifest or justify in console         |

---

## 13. Future improvements

- Automate AAB upload via [Google Play Developer Publishing API](https://developers.google.com/android-publisher) from CI.
- Promote to Closed track once tester count > 50.
- Add release notes generated from git log.
