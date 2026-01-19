from stable_baselines3 import PPO
from stable_baselines3.common.vec_env import DummyVecEnv
from ai_atc_env import SimpleATCEnv

num_envs = 32
env = DummyVecEnv([lambda: SimpleATCEnv(max_episode_steps=50) for _ in range(num_envs)])

model = PPO(
    "MlpPolicy",
    env,
    verbose=1,
    tensorboard_log="./tb_logs",
    device="cpu"  # recommended for MLPs
)

model.learn(total_timesteps=50_000)
model.save("ppo_ai_atc_fast")
