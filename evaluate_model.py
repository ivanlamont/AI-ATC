import numpy as np

def evaluate_model(model, env, n_eval_episodes=20):
    episode_rewards = []

    for ep in range(n_eval_episodes):
        obs = env.reset()
        done = False
        ep_reward = 0.0

        while not done:
            action, _ = model.predict(obs, deterministic=True)
            obs, rewards, dones, infos = env.step(action)

            # VecEnv: rewards and dones are arrays
            ep_reward += float(rewards[0])
            done = bool(dones[0])

        episode_rewards.append(ep_reward)

    mean_reward = np.mean(episode_rewards)
    std_reward = np.std(episode_rewards)

    print(f"Eval over {n_eval_episodes} episodes:")
    print(f"  Mean reward: {mean_reward:.2f} +/- {std_reward:.2f}")

    return mean_reward
