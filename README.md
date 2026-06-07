# 🎵 EduMusic – Server

> An educational music management and classification system with automatic tagging, Hebrew NLP, and AI-powered song processing.

![C#](https://img.shields.io/badge/C%23-.NET%208-blueviolet?logo=dotnet)
![Python](https://img.shields.io/badge/Python-3.11-blue?logo=python)
![Flask](https://img.shields.io/badge/Flask-3.x-lightgrey?logo=flask)
![SQL Server](https://img.shields.io/badge/SQL%20Server-EF%20Core-red?logo=microsoftsqlserver)
![License](https://img.shields.io/badge/license-Private-lightgrey)

---

## 📖 Table of Contents

1. [About the Project](#-about-the-project)
2. [Architecture](#-architecture)
3. [Tech Stack](#-tech-stack)
4. [Song Processing Pipeline](#-song-processing-pipeline)
5. [Algorithms & AI](#-algorithms--ai)
6. [Python NLP Server](#-python-nlp-server)
7. [Project Structure](#-project-structure)
8. [API Reference](#-api-reference)
9. [Getting Started](#-getting-started)
10. [Environment Variables](#-environment-variables)
11. [Database Schema](#-database-schema)
12. [Development Notes](#-development-notes)

---

## 🎯 About the Project

**EduMusic** is a backend server for an educational music management platform. The system allows teachers to upload Hebrew-language songs and automatically processes each one through a fully AI-driven pipeline:

- **Vocal Separation** — strips the vocal track from the musical accompaniment (UVR-MDX-NET deep learning model)
- **Speech Transcription** — converts audio to raw Hebrew text (Groq Whisper API)
- **Spelling Correction** — fixes transcription and orthographic errors (LLaMA 3.3 70B via Groq)
- **Morphological Analysis** — performs full Hebrew NLP: tokenization, POS tagging, and lemmatization (Stanford Stanza)
- **Automatic Classification** — assigns each song to an educational category (Multinomial Naive Bayes)

The entire pipeline runs asynchronously as a .NET Background Worker with real-time job status tracking.

---

## 🏗 Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                        Client (React)                        │
└───────────────────────────┬──────────────────────────────────┘
                            │ HTTP / JWT
┌───────────────────────────▼──────────────────────────────────┐
│               ASP.NET Core Web API  (C#)                     │
│  ┌────────────┐  ┌────────────┐  ┌─────────────────────────┐ │
│  │ Controllers│  │  Services  │  │  Background LyricsWorker│ │
│  └────────────┘  └────────────┘  └───────────┬─────────────┘ │
│                                               │               │
│  ┌───────────────────────────────────────────▼─────────────┐ │
│  │                    LyricsProcessor                       │ │
│  │  1. VocalSeparator → 2. Groq Whisper → 3. Groq LLaMA   │ │
│  │  4. Python NLP     → 5. TagService   → 6. Classifier    │ │
│  └──────────────────────────────────────────────────────────┘ │
└───────────────────────┬──────────────────────────────────────┘
                        │                        │
         ┌──────────────▼───────┐    ┌───────────▼───────────┐
         │   SQL Server         │    │  Python Flask Server   │
         │   (Entity Framework) │    │  (Stanza Hebrew NLP)   │
         └──────────────────────┘    └───────────────────────┘
```

---

## 🛠 Tech Stack

### Primary Backend — C# / ASP.NET Core

| Library / Framework | Purpose |
|---------------------|---------|
| ASP.NET Core 8 | Web API, Dependency Injection, BackgroundService |
| Entity Framework Core | ORM, SQL Server, Code-First migrations |
| JWT Bearer Auth | Authentication for teachers (Admin) and students (User) |
| TagLib# | Reads audio metadata from uploaded files (artist, title, duration) |
| AutoMapper | DTO ↔ Entity mapping |
| ExcelDataReader | Bulk student provisioning from Excel files |
| Swagger / OpenAPI | Interactive API documentation |
| HttpClient | Communication with Groq API and Python NLP server |

### NLP Microservice — Python / Flask

| Library | Purpose |
|---------|---------|
| Flask 3 | Lightweight web server with Blueprint routing |
| Stanza (Stanford NLP) | Hebrew morphological analysis (tokenize, MWT, POS, lemma) |
| audio-separator | Vocal isolation using the UVR-MDX-NET-Inst_HQ_3 ONNX model |
| ffmpeg | Post-processing: WAV → MP3 conversion |

### External AI Services

| Service | Model | Usage |
|---------|-------|-------|
| Groq API | `whisper-large-v3` | Audio transcription to Hebrew text |
| Groq API | `llama-3.3-70b-versatile` | Spelling and transcription error correction |

---

## 🔄 Song Processing Pipeline

When a teacher uploads a song, a `JobState` record is created and the `LyricsWorker` background service picks it up for processing through eight sequential steps:

```
[UPLOAD]
   │
   ▼
Step 1 — SeparatingVocals
   └─ VocalSeparatorService → Python /separate-vocals
   └─ Model: UVR-MDX-NET-Inst_HQ_3.onnx (CPU inference)
   └─ Output: WAV → converted to MP3 (ffmpeg, 16kHz mono, 64kbps)
   │
   ▼
Step 2 — Transcribing
   └─ GroqApiClient → whisper-large-v3
   └─ Language hint: Hebrew (he)
   └─ Segment filtering: avg_logprob > -1.0, no_speech_prob < 0.6
   │
   ▼
Step 3 — FixingLyrics
   └─ GroqApiClient → llama-3.3-70b-versatile
   └─ Structured prompt: fix Hebrew spelling, word boundaries, transcription artefacts
   │
   ▼
Step 4 — NormalizingWords
   └─ NlpClientService → Python /extract
   └─ Stanza pipeline: tokenize → mwt → pos → lemma
   └─ Returns: wordCounts { lemma: frequency }
   │
   ▼
Step 5 — SynchronizingTags
   └─ TagService: noise filtering, Hebrew-only validation, upsert to DB
   └─ Hebrew check: Unicode range \u0590–\u05FF
   │
   ▼
Step 6 — Classifying
   └─ ClassificationService → Multinomial Naive Bayes
   └─ Selects the leaf category with the highest log-probability score
   │
   ▼
Step 7 — Persisting Results
   └─ Saves lyrics, assigned category, and tag frequencies to the database
   └─ Updates JobState → Completed
```

---

## 🧠 Algorithms & AI

### 1. Multinomial Naive Bayes — Song Classification

`ClassificationService.cs` implements a **Multinomial Naive Bayes** classifier that predicts the most appropriate educational category for a song based on its extracted word tags.

#### The Scoring Formula

```
score(Category) = log P(Category) + Σ [ count(tag) × log P(tag | Category) ]
```

#### Step 1 — Prior Probability

Uses **Laplace smoothing** to prevent zero-probability for categories with no training data:

```
log P(Category) = log( (songs_in_category + 1) / (total_songs + num_categories) )
```

#### Step 2 — Likelihood

For each tag in the song, computes its conditional probability given a category:

```
P(tag | Category) = (freq(tag, Category) + 1) / (totalWords(Category) + VocabularySize)
```

- `freq(tag, Category)` — cumulative occurrences of this tag across all songs in the category
- `VocabularySize` — total number of unique tags across the entire system
- The **+1 additive smoothing** prevents log(0) for unseen tag–category combinations
- Each tag's log-probability is **weighted by its frequency** in the current song (`count(tag)`)

#### Step 3 — Category Selection

```
Best = argmax score(Category)
```

Classification is performed only over **leaf nodes** of the category hierarchy, scoped to the uploading admin's categories plus global categories.

#### In-Memory Cache (`IClassificationDataCache`)

To avoid repeated database queries during classification, key statistics are cached:

| Cache Key | Description |
|-----------|-------------|
| `TotalSongs` | Total number of songs in the system |
| `SongsPerCategory` | Song count per category ID |
| `CategoryTagCounts` | TagID → cumulative frequency, per category |
| `VocabularySize` | Total number of distinct tags globally |

---

### 2. Hebrew NLP Pipeline — Stanza

`StanzaService.py` runs a full Stanford Stanza pipeline configured for Modern Hebrew:

```
Raw lyrics text
    │
    ▼  tokenize    —  Splits text into sentences and word tokens
    │
    ▼  mwt         —  Multi-Word Token expansion
    │                  e.g. "שבו" → ["ש", "בו"] (agglutinative Hebrew forms)
    │
    ▼  pos          —  Part-of-Speech tagging
    │                  (NOUN, VERB, ADJ, ADP, CCONJ, DET, PRON, PUNCT ...)
    │
    ▼  lemma        —  Morphological root extraction
    │                  e.g. "ילדים" → "ילד"
    │
    ▼  StopWords    —  Dual-layer filtering:
    │                  1. POS filter: PUNCT, NUM, ADP, CCONJ, SCONJ, DET,
    │                                 PRON, AUX, INTJ, PART, X
    │                  2. Lexical filter: Hebrew stop-word list
    │                     (prepositions, pronouns, conjunctions,
    │                      filler sounds common in song lyrics, etc.)
    │
    ▼  Counter      —  Frequency count of each remaining lemma
    │
    ▼  { "wordCounts": { "ילד": 5, "שמח": 3, "אהבה": 8 } }
```

Both `StanzaService` and `VocalSeparatorService` are **singletons** — the Stanza model and the UVR ONNX model are loaded once at server startup to avoid repeated cold-start costs.

---

### 3. Groq Whisper — Noise-Filtered Transcription

`GroqApiClient.cs` applies a quality-filtering layer on top of raw Whisper output before returning the transcript:

| Parameter | Threshold | Effect |
|-----------|-----------|--------|
| `avg_logprob` | > −1.0 | Segments with low model confidence are discarded |
| `no_speech_prob` | < 0.6 | Silent or non-speech segments are discarded |
| `MinCoverageRatio` | 0.60 | At least 60% of the song's duration must be covered by valid segments |
| `MaxGapSeconds` | 4.0 | Gaps larger than 4 seconds between accepted segments are flagged |

---

### 4. LLaMA 3.3 70B — Transcription Correction

After Whisper transcription, the raw Hebrew text is sent to `llama-3.3-70b-versatile` with a structured prompt that corrects:

- Hebrew spelling errors introduced during transcription
- Words that were incorrectly merged or split
- Misidentified roots and word forms typical of ASR errors in Hebrew

---

### 5. UVR-MDX-NET — Vocal Source Separation

`VocalSeparatorService.py` uses `UVR-MDX-NET-Inst_HQ_3.onnx` — a deep learning model based on the MDX-Net architecture, optimized for CPU inference without a GPU requirement. The model performs blind source separation and returns two output streams: **vocals** and **instrumental**. The vocals file is then converted from WAV to MP3 (16 kHz, mono, 64 kbps) via ffmpeg to reduce file size before sending to Whisper.

---

## 🐍 Python NLP Server

A standalone Flask application listening on `port 5000`, registered as a named `HttpClient` in the ASP.NET Core DI container via `NlpClientService`.

### Endpoints

#### `POST /extract`

Accepts raw Hebrew lyrics text, runs the full Stanza morphological pipeline, and returns lemma frequencies.

**Request body:**
```json
{ "text": "song title and lyrics here..." }
```

**Response:**
```json
{ "wordCounts": { "child": 5, "happy": 3, "love": 8 } }
```

#### `POST /separate-vocals`

Runs vocal separation on a given audio file using the UVR-MDX-NET model.

**Request body:**
```json
{ "audio_path": "/absolute/path/to/song.mp3" }
```

**Response:**
```json
{ "vocals_path": "/absolute/path/to/song_(Vocals).mp3" }
```

### Python Server Structure

```
PythonNLP/
├── server.py                          # Entry point — runs Flask app on port 5000
├── config.py                          # Config: language, Stanza processors
├── requirements.txt                   # flask, stanza, audio-separator[cpu]
└── app/
    ├── __init__.py                    # Flask application factory + blueprint registration
    ├── routes/
    │   ├── extract_routes.py          # POST /extract
    │   └── vocal_routes.py            # POST /separate-vocals
    └── services/
        ├── stanza_service.py          # Singleton: full Hebrew NLP pipeline
        ├── vocal_separator_service.py # Singleton: UVR-MDX-NET model wrapper
        └── stopwords_service.py       # Stop-word list + POS-based filtering logic
```

---

## 📁 Project Structure

```
EduMusic-server/
├── EduMusic/                          # ASP.NET Core Web API entry point
│   ├── Program.cs                     # DI registration, JWT, CORS, Swagger, Worker
│   └── Background/
│       └── LyricsWorker.cs            # BackgroundService — polls DB for queued jobs
│
├── Service/                           # Business Logic Layer
│   ├── Interfaces/                    # Service contracts (ISongService, IClassificationService …)
│   └── Services/
│       ├── LyricsProcessor.cs         # Orchestrates the full 8-step pipeline
│       ├── ClassificationService.cs   # Multinomial Naive Bayes classifier
│       ├── TagService.cs              # Tag normalization, Hebrew validation, DB upsert
│       ├── SongService.cs             # File upload, metadata extraction, job creation
│       ├── UserService.cs             # Student provisioning + Excel bulk import
│       ├── CategoryService.cs         # Category CRUD
│       ├── NlpClientService.cs        # HTTP client wrapper for Python /extract
│       ├── GroqApiClient.cs           # Whisper transcription + LLaMA correction via Groq
│       └── TokenService.cs            # JWT generation for Admin and User roles
│
├── Repository/                        # Data Access Layer
│   ├── Entities/                      # EF Core entity models
│   │   ├── Song.cs
│   │   ├── Category.cs
│   │   ├── Tag.cs
│   │   ├── User.cs
│   │   ├── Admin.cs
│   │   ├── JobState.cs
│   │   ├── SongTagFrequency.cs
│   │   └── TagCategory.cs
│   ├── Interfaces/                    # Repository contracts
│   └── Repositories/                  # EF Core generic + specialized implementations
│
├── DataContext/                       # EF Core DbContext (EduMusicContext)
│
├── Common/                            # Shared DTOs, Enums, Custom Exceptions
│   ├── Dto/
│   └── enums/
│       ├── SongStatus.cs              # Pending | ExtractingLyrics | Classifying | Done | Failed
│       └── JobStatus.cs               # Queued | SeparatingVocals | Transcribing |
│                                      # FixingLyrics | NormalizingWords |
│                                      # SynchronizingTags | Classifying | Completed | Failed
│
└── PythonNLP/                         # Python Flask NLP microservice
    ├── server.py
    ├── config.py
    ├── requirements.txt
    └── app/
```

---

## 📡 API Reference

> All endpoints except `POST /api/auth/login` and `POST /api/auth/register` require a valid **JWT Bearer** token in the `Authorization` header.

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/auth/login` | Authenticate a teacher or student; returns a signed JWT |
| `POST` | `/api/auth/register` | Register a new teacher (Admin) account |

### Songs

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/songs/upload` | Upload an audio file; triggers the full processing pipeline |
| `GET` | `/api/songs` | Retrieve the list of songs for the authenticated user |
| `GET` | `/api/songs/{id}` | Retrieve details for a specific song |
| `PUT` | `/api/songs/{id}/category` | Manually reassign a song's category (Admin only) |

### Categories

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/categories` | List all categories (global + admin-specific) |
| `POST` | `/api/categories` | Create a new category (Admin only) |

### Users (Students)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/users/manual` | Provision students individually via JSON |
| `POST` | `/api/users/import` | Bulk-import students from an `.xlsx` file |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server LocalDB
- Python 3.11+
- `ffmpeg` available on the system `PATH`

### C# API Server

```bash
# 1. Clone the repository
git clone https://github.com/ShiraRozental/EduMusic-server.git
cd EduMusic-server

# 2. Restore NuGet packages
dotnet restore

# 3. Configure appsettings.json (see Environment Variables section below)

# 4. Apply EF Core migrations
dotnet ef database update --project DataContext --startup-project EduMusic

# 5. Run the API
dotnet run --project EduMusic
```

The API will start on `https://localhost:7xxx` with Swagger UI available at `/swagger`.

### Python NLP Server

```bash
cd PythonNLP

# 1. Install Python dependencies
pip install -r requirements.txt

# 2. Download the Hebrew Stanza model (first run only)
python -c "import stanza; stanza.download('he')"

# 3. Start the Flask server
python server.py
```

The NLP server will listen on `http://0.0.0.0:5000`.

> Both servers must be running simultaneously for the pipeline to function correctly.

---

## 🔐 Environment Variables

Configure the following in `appsettings.json` (or `appsettings.Development.json` for local development):

```json
{
  "ConnectionStrings": {
    "database-home": "Server=localhost;Database=EduMusic;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG",
    "Issuer": "EduMusic",
    "Audience": "EduMusicClient"
  },
  "NlpService": {
    "BaseUrl": "http://localhost:5000/"
  },
  "GroqApiKey": "YOUR_GROQ_API_KEY",
  "Worker": {
    "PollingDelayMs": 2000
  }
}
```

| Key | Description |
|-----|-------------|
| `ConnectionStrings:database-home` | SQL Server connection string |
| `Jwt:Key` | HMAC-SHA256 signing secret (minimum 32 characters) |
| `Jwt:Issuer` / `Jwt:Audience` | JWT issuer and audience claim values |
| `NlpService:BaseUrl` | Base URL of the Python Flask server |
| `GroqApiKey` | API key from [console.groq.com](https://console.groq.com) |
| `Worker:PollingDelayMs` | Polling interval for the background job worker (ms) |

---

## 🗄 Database Schema

### Core Entities

```
Song
├── SongID          (PK)
├── Title           (string, 150)
├── Artist          (string, 100)
├── FilePath        (string)
├── RawLyrics       (nvarchar max, nullable)
├── Duration        (int, seconds)
├── Status          (SongStatus enum)
├── UploadDate      (DateTime)
├── CategoryID      (FK → Category, nullable)
└── UploaderID      (FK → Admin)

Category
├── CategoryID      (PK)
├── CategoryName    (string, 2–50)
├── ParentCategoryID (FK → Category, nullable — self-referencing tree)
└── AdminID         (FK → Admin, nullable — null = global category)

Tag
├── TagID           (PK)
├── TagText         (string, 50, Hebrew lemma)
├── → SongTagFrequency  (many-to-many: Tag ↔ Song + frequency count)
└── → TagCategory       (many-to-many: Tag ↔ Category)

JobState
├── Id              (Guid, PK)
├── SongID          (FK → Song)
├── Status          (JobStatus enum)
├── CreatedAt       (DateTime)
└── CompletedAt     (DateTime, nullable)

User
├── UserID          (PK)
├── ID              (string, exactly 9 digits — national ID)
├── FullNameUser    (string, 2–100)
└── MyTeacherID     (FK → Admin)
```

### Job Status Flow

```
Queued → SeparatingVocals → Transcribing → FixingLyrics
      → NormalizingWords → SynchronizingTags → Classifying → Completed
                                                            → Failed
```

---

## 📝 Development Notes

- All C# services are registered via an `AddServices()` extension method in `Program.cs`
- `StanzaService` and `VocalSeparatorService` are Python **singletons** — models are loaded once at startup; subsequent requests reuse the loaded instance
- Maximum audio upload size is **500 MB**, enforced on both Kestrel and IIS server options
- `LyricsWorker` polls the database every `PollingDelayMs` milliseconds (default: 2,000 ms) and processes one job per cycle
- JWT claims include a `Role` claim (`"Admin"` or `"User"`) enabling endpoint-level authorization guards
- The category tree is hierarchical (self-referencing FK); classification targets only **leaf nodes**

---

## 🤝 Contributors

| Name | Role |
|------|------|
| Shira Rozental | Full-Stack Developer |

---

*Final year software engineering project.*
