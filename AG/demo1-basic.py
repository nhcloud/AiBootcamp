import asyncio
import os
from dotenv import load_dotenv
from autogen_agentchat.agents import AssistantAgent
from autogen_agentchat.ui import Console
from autogen_ext.models.ollama import OllamaChatCompletionClient  # Import Ollama client

load_dotenv()


model_client = OllamaChatCompletionClient(
    model=os.getenv("LOCAL_MODEL_NAME"),
    api_key=os.getenv("LOCAL_MODEL_API_KEY")
)

agent = AssistantAgent(
    name="assistant",
    system_message="You are a helpful assistant.",
    model_client=model_client,
    model_client_stream=True,
)


async def main() -> None:
    task = input("Enter the task message: ") 
    await Console(agent.run_stream(task=task))

asyncio.run(main())