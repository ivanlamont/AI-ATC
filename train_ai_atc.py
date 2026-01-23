import os
from stable_baselines3 import PPO
from stable_baselines3.common.vec_env import DummyVecEnv, VecNormalize, VecMonitor
from ai_atc_env import AIATCEnv
from constants import LOG_DIR, MODEL_DIR
from visualize_ai_atc import create_visualization

def perform_training():
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
    saved_model = f"{MODEL_DIR}/ai_atc_ppo"
    model.save(saved_model)
    env.save(f"{MODEL_DIR}/vecnormalize.pkl")

    env.close()

    return saved_model

if __name__ == "__main__":
    model_path = perform_training()
    create_visualization(model_path=model_path)