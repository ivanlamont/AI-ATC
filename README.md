# ✈️ AI-ATC — Reinforcement Learning Air Traffic Control Simulator

This project implements a physics-based, multi-aircraft air traffic control (ATC) simulator trained using Proximal Policy Optimization (PPO). The system models hierarchical control, where the RL agent issues high-level ATC clearances (heading, speed, altitude), and a simulated pilot executes these clearances via realistic control laws and aircraft dynamics.

The project focuses on reward shaping, curriculum learning, safety constraints, and control abstraction to solve a sparse-reward, safety-critical, multi-agent control problem.

---

## 🚀 Project Goals

* Designed hierarchical RL architecture separating decision policy from low-level control
* Implemented curriculum learning across flight phases (vectoring, descent, approach, landing)
* Engineered dense shaping rewards to solve sparse terminal objective
* Built physics-based aircraft dynamics with envelope protection
* Designed discrete ATC-style action space to stabilize PPO training
* Implemented multi-agent separation constraints and safety penalties
* Developed evaluation and visualization pipeline for trained policies
* Built unit tests for reward functions and environment correctness

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
* VS Code with Dev Containers



## �️ Roadmap

* Integration with the real world - pulling live aircraft from ADSBexchange and pulling real-world approach plates
* Add a UI that allows humans to stream audio via STT to compete with the trained engine for landings per hour
* Add wind, weather, runway changes




