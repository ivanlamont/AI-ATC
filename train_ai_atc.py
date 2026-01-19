import os
from stable_baselines3 import PPO
from stable_baselines3.common.vec_env import DummyVecEnv, VecNormalize, VecMonitor
from ai_atc_env import AIATCEnv

LOG_DIR = "tensorboard"
MODEL_DIR = "models"
os.makedirs(LOG_DIR, exist_ok=True)
os.makedirs(MODEL_DIR, exist_ok=True)

def make_env():
    return AIATCEnv()

# --- Vectorized env ---
env = DummyVecEnv([make_env])

# --- Monitor BEFORE normalization ---
env = VecMonitor(env, filename=os.path.join(LOG_DIR, "monitor.csv"))

# --- Normalize observations ---
env = VecNormalize(
    env,
    norm_obs=True,
    norm_reward=False,   # keep rewards interpretable
    clip_obs=10.0
)

model = PPO(
    policy="MlpPolicy",
    env=env,
    verbose=1,
    n_steps=2048,
    batch_size=256,
    learning_rate=3e-4,
    device="cpu",
    tensorboard_log=LOG_DIR,
)

model.learn(total_timesteps=1_000_000)

# --- Save model AND normalization stats ---
model.save(f"{MODEL_DIR}/ai_atc_ppo")
env.save(f"{MODEL_DIR}/vecnormalize.pkl")

env.close()
