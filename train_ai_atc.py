import os
from stable_baselines3 import PPO
from stable_baselines3.common.vec_env import DummyVecEnv, VecNormalize, VecMonitor
from ai_atc_env import AIATCEnv
from constants import LOG_DIR, MODEL_DIR, MODEL_OUTPUT
from evaluate_model import evaluate_model
from visualize_ai_atc import create_visualization
from curriculum import AdaptiveCurriculum, train_with_adaptive_curriculum
import warnings

warnings.filterwarnings(
    "ignore",
    message=".*Protobuf gencode version.*"
)

stage_configs = [
    {"stage": 0, "timesteps": 200_000, "target_reward": 0.5},
    {"stage": 1, "timesteps": 300_000, "target_reward": 1.0},
    {"stage": 2, "timesteps": 400_000, "target_reward": 1.5},
    {"stage": 3, "timesteps": 500_000, "target_reward": 2.0},
    {"stage": 4, "timesteps": 600_000, "target_reward": 2.5},
    {"stage": 5, "timesteps": 800_000, "target_reward": None},
]

def perform_training(use_curriculum=True, use_adaptive_curriculum=False):
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

    if use_adaptive_curriculum:
        # Use enhanced adaptive curriculum
        curriculum = AdaptiveCurriculum()
        model, curriculum = train_with_adaptive_curriculum(
            model,
            env,
            curriculum=curriculum,
            total_episodes=2000,
            eval_interval=100,
            verbose=True
        )
    elif use_curriculum:
        # Use basic curriculum
        train_with_curriculum(model, env, stage_configs)
    else:
        model.learn(total_timesteps=1_000_000)

    # --- Save model AND normalization stats ---
    saved_model = MODEL_OUTPUT
    model.save(saved_model)
    env.save(f"{MODEL_DIR}/vecnormalize.pkl")

    env.close()

    return saved_model

def train_with_curriculum(model, env, stage_configs):

    for cfg in stage_configs:
        stage = cfg["stage"]
        timesteps = cfg["timesteps"]

        print(f"\n===== Training curriculum stage {stage} =====")
        env.env_method("set_curriculum_stage", stage)

        model.learn(total_timesteps=timesteps, reset_num_timesteps=False)

        # Optional evaluation here
        mean_reward = evaluate_model(model, env, n_eval_episodes=20)
        print(f"Stage {stage} mean reward: {mean_reward:.3f}")

        if cfg.get("target_reward") is not None:
            if mean_reward < cfg["target_reward"]:
                print("⚠️ Stage not mastered — consider more timesteps")


if __name__ == "__main__":
    import sys

    use_adaptive = "--adaptive" in sys.argv
    use_curriculum = "--no-curriculum" not in sys.argv

    print(f"Training configuration:")
    print(f"  Use curriculum: {use_curriculum}")
    print(f"  Use adaptive curriculum: {use_adaptive}")

    model_path = perform_training(
        use_curriculum=use_curriculum,
        use_adaptive_curriculum=use_adaptive
    )
    create_visualization(model_path=model_path)