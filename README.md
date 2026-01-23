# ✈️ AI-ATC — Reinforcement Learning Air Traffic Control Simulator

AI-ATC is an experimental reinforcement learning project that explores how an AI agent can act as a simplified **air traffic controller**, issuing navigation instructions to guide an aircraft toward an airport using the fewest possible commands.

The project is intentionally minimal at first, focusing on **fast iteration, correctness, and learning dynamics**, with a roadmap toward more realistic air traffic control scenarios.

---

## 🚀 Project Goals

* Demonstrate **applied reinforcement learning** skills using modern tooling
* Build a controllable, extensible simulation environment
* Train an agent to efficiently guide an aircraft to a destination
* Provide clear visualization of learned behavior
* Scale from a toy problem to a realistic ATC simulation

---

## 🧠 Core Concept

* **Environment**

  * One airport (fixed location)
  * One airplane
  * 2D space (latitude, longitude only)
  * Discrete time steps (simulation clock)

* **Agent (ATC) Actions**

  * Turn left
  * Turn right
  * Speed up
  * Slow down
  * Maintain heading/speed

* **Objective**

  * Reach the airport
  * Minimize number of instructions
  * Avoid overshooting or oscillation

This is modeled as a **reinforcement learning problem** using PPO (Proximal Policy Optimization).

---

## 🧰 Technology Stack

* **Python 3.11**
* **TensorFlow 2.20 (GPU)**
* **Stable-Baselines3**
* **Gymnasium**
* **Docker + NVIDIA Container Toolkit**
* **Matplotlib** (visualization)

GPU acceleration is supported for model training, while visualization runs on the CPU.

---

## 📁 Repository Structure

```
AI-ATC/
├── ai_atc_env.py          # Custom Gymnasium environment
├── train_ai_atc.py        # PPO training script
├── visualize_ai_atc.py    # Post-training visualization / animation
├── Dockerfile             # GPU-enabled dev container
├── requirements.txt       # Python dependencies
├── .gitignore
└── README.md
```

---

## 🐳 Running with Docker (GPU)

### Prerequisites

* NVIDIA GPU (tested on RTX 40-series)
* NVIDIA drivers installed
* Docker + NVIDIA Container Toolkit
* VS Code with Dev Containe
