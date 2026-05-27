import logging
import os

logger = logging.getLogger(__name__)


class VocalSeparatorService:
    """
    Separates vocals from music using audio-separator (UVR-MDX-NET model).
    Returns the path to the vocals-only wav file.
    """

    # MDX-Net model — best quality/speed tradeoff on CPU, no GPU needed
    MODEL_NAME = "UVR-MDX-NET-Inst_HQ_3.onnx"

    def __init__(self):
        # import here so the server starts even if audio-separator is not installed
        try:
            from audio_separator.separator import Separator
            self._Separator = Separator
            logger.info("audio-separator loaded successfully")
        except ImportError:
            self._Separator = None
            logger.warning(
                "audio-separator not installed — VocalSeparatorService unavailable. "
                "Install with: pip install audio-separator[cpu]"
            )

    def separate_vocals(self, audio_path: str) -> str:
        """
        Runs vocal separation and returns the path to the vocals file.
        Raises RuntimeError if the library is not installed.
        """
        if self._Separator is None:
            raise RuntimeError(
                "audio-separator is not installed. "
                "Run: pip install audio-separator[cpu]"
            )

        if not os.path.exists(audio_path):
            raise FileNotFoundError(f"Audio file not found: {audio_path}")

        output_dir = os.path.dirname(audio_path)
        base_name = os.path.splitext(os.path.basename(audio_path))[0]

        logger.info("Starting vocal separation for: %s", os.path.basename(audio_path))

        separator = self._Separator(
            model_file_dir=os.path.join(output_dir, "_models"),
            output_dir=output_dir,
            output_format="WAV",
            normalization_threshold=0.9,
        )
        separator.load_model(self.MODEL_NAME)

        # returns list of output file paths: [vocals_path, instrumental_path]
        output_files: list[str] = separator.separate(audio_path)

        # find the vocals file — audio-separator names it with "(Vocals)" suffix
        vocals_path = next(
            (f for f in output_files if "Vocals" in f or "vocals" in f),
            output_files[0] if output_files else None
        )

        if not vocals_path:
            raise RuntimeError(...)

        # חיבור נתיב מלא אם הוחזר רק שם קובץ
        if not os.path.isabs(vocals_path):
            vocals_path = os.path.join(output_dir, vocals_path)

        if not os.path.exists(vocals_path):
            raise RuntimeError(
                f"Vocal separation produced no output for {audio_path}. "
                f"Files returned: {output_files}"
            )

        if not vocals_path or not os.path.exists(vocals_path):
            raise RuntimeError(
                f"Vocal separation produced no output for {audio_path}. "
                f"Files returned: {output_files}"
            )

        logger.info("Vocal separation complete: %s", os.path.basename(vocals_path))

        # המרה ל-MP3 לפני החזרה
        vocals_path = self._convert_to_mp3(vocals_path)
        logger.info("Converted to MP3: %s", os.path.basename(vocals_path))

        return vocals_path


    def _convert_to_mp3(self, wav_path: str) -> str:
        """Convert WAV to MP3 to reduce file size before transcription."""
        import subprocess
        mp3_path = wav_path.replace(".wav", ".mp3")
        subprocess.run([
            "ffmpeg", "-i", wav_path,
            "-b:a", "64k",
            "-ar", "16000",
            "-ac", "1",
            "-y", mp3_path
        ], check=True)
        os.remove(wav_path)
        return mp3_path


# Singleton
vocal_separator_service = VocalSeparatorService()
