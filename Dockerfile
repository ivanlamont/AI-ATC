FROM tensorflow/tensorflow:2.20.0-gpu

RUN apt-get update && apt-get install -y ffmpeg

WORKDIR /app

COPY requirements.txt .
RUN pip install -r requirements.txt \
    && rm -rf /root/.cache/pip  # optional cleanup to reduce image size

COPY *.py .

ENV TF_FORCE_GPU_ALLOW_GROWTH=true
ENV TF_CPP_MIN_LOG_LEVEL=2

CMD ["python", "train_ai_atc.py"]
