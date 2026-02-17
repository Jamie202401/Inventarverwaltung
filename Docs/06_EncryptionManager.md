# EncryptionManager — AES-256 Verschlüsselung

**Datei:** `Encryptionmanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class EncryptionManager`

---

## Zweck

`EncryptionManager` stellt AES-256-Verschlüsselung für alle sicherheitskritischen Daten der Anwendung bereit — primär für Log-Dateien. Alle Verschlüsselungs- und Entschlüsselungsoperationen sind gekapselt und können ohne Kenntnis der Kryptografie-Details verwendet werden.

---

## Sicherheitsdetails

| Parameter | Wert |
|---|---|
| Algorithmus | AES-256 (Advanced Encryption Standard) |
| Schlüssellänge | 256 Bit (32 Bytes) |
| Betriebsmodus | CBC (Cipher Block Chaining) |
| Padding | PKCS7 |
| Key Derivation | PBKDF2 mit SHA-256, 10.000 Iterationen |
| IV | 128 Bit, fest kodiert |
| Salt | `InventarSalt2026!` (UTF-8) |

> ⚠️ In einer Produktivumgebung sollte der Schlüssel extern sicher gespeichert werden (z.B. in einem Key-Vault oder einer Umgebungsvariable).

---

## Interne Felder (private)

| Feld | Typ | Beschreibung |
|---|---|---|
| `encryptionKey` | `byte[]` | 256-Bit-Schlüssel, abgeleitet via PBKDF2 |
| `iv` | `byte[16]` | Fest kodierter Initialisierungsvektor |

---

## Methoden

### `DeriveKeyFromPassword(password) → byte[]` *(private)*

Leitet einen sicheren 256-Bit-Schlüssel aus dem übergebenen Passwort mittels PBKDF2 (Rfc2898DeriveBytes) und SHA-256 ab. Wird einmalig beim Klasseninitialisierung aufgerufen.

---

### `EncryptString(plainText) → byte[]`

Verschlüsselt einen Klartext-String nach AES-256-CBC und gibt das Ergebnis als Byte-Array zurück. Gibt `null` zurück bei leerem Input oder Fehler.

---

### `DecryptBytes(cipherBytes) → string`

Entschlüsselt ein Byte-Array zurück in Klartext. Gibt `null` zurück bei leerem Input oder Entschlüsselungsfehler.

---

### `EncryptFile(inputFile, outputFile) → bool`

Liest eine Klartextdatei, verschlüsselt sie und schreibt das Ergebnis in eine neue Datei. Gibt `true` bei Erfolg zurück.

---

### `DecryptFile(inputFile, outputFile) → bool`

Liest eine verschlüsselte Datei, entschlüsselt sie und schreibt den Klartext in eine neue Datei. Gibt `true` bei Erfolg zurück.

---

### `EncryptFileInPlace(filePath) → bool`

Verschlüsselt eine Datei an Ort und Stelle: schreibt zunächst in eine `.tmp`-Datei, löscht dann das Original und benennt die `.tmp`-Datei um. Räumt bei Fehler auf.

---

### `DecryptFileInPlace(filePath) → bool`

Entschlüsselt eine Datei an Ort und Stelle. Gleiches Muster wie `EncryptFileInPlace`.

---

### `AppendEncrypted(filePath, textToAdd) → bool`

Fügt neuen Klartext zu einer bestehenden verschlüsselten Datei hinzu:
1. Existierende Datei wird entschlüsselt
2. Neuer Text wird angehängt
3. Gesamter Inhalt wird neu verschlüsselt und gespeichert

---

### `ReadEncryptedFile(filePath) → string`

Liest und entschlüsselt eine vollständige verschlüsselte Datei. Gibt `null` zurück wenn die Datei nicht existiert oder ein Fehler auftritt.

---

### `GetFileHash(filePath) → string`

Berechnet einen SHA-256-Hash der Datei zur Integritätsprüfung. Gibt den Hash als Hex-String in Kleinbuchstaben zurück (z.B. `a3f2...`). Gibt `null` bei Fehler zurück.

---

### `ZeigeVerschluesselungsInfo()`

Zeigt eine formatierte Konsolen-Übersicht aller Sicherheitsparameter an.

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `ConsoleHelper` | Fehlerausgaben |
| `System.Security.Cryptography` | AES, SHA256, Rfc2898DeriveBytes |
