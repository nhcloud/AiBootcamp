import asyncio
import os
from dotenv import load_dotenv
from autogen_agentchat.agents import AssistantAgent
from autogen_agentchat.base import TaskResult
from autogen_agentchat.conditions import ExternalTermination, TextMentionTermination
from autogen_agentchat.teams import RoundRobinGroupChat
from autogen_agentchat.ui import Console
from autogen_core import CancellationToken
from autogen_ext.models.openai import OpenAIChatCompletionClient
from autogen_ext.models.openai import OpenAIChatCompletionClient
from autogen_ext.models.openai import AzureOpenAIChatCompletionClient
from autogen_ext.models.ollama import OllamaChatCompletionClient  # Import Ollama client
load_dotenv()
# Create an OpenAI model client.
model_client = AzureOpenAIChatCompletionClient(
    azure_deployment=os.getenv("DEPLOYMENT_NAME"),
    model=os.getenv("MODEL_NAME"),
    api_version=os.getenv("API_VERSION"),
    azure_endpoint=os.getenv("ENDPOINT_URI"),
    api_key=os.getenv("API_KEY")
)

# Create the primary agent.
primary_agent = AssistantAgent(
    "primary",
    model_client=model_client,
    system_message="You are a helpful AI assistant.",
)

# Create the critic agent.
critic_agent = AssistantAgent(
    "critic",
    model_client=model_client,
    system_message="Provide constructive feedback. Respond with 'APPROVE' to when your feedbacks are addressed.",
)

# Define a termination condition that stops the task if the critic approves.
text_termination = TextMentionTermination("APPROVE")

# Create a team with the primary and critic agents.
team = RoundRobinGroupChat([primary_agent, critic_agent], termination_condition=text_termination,max_turns=4)
# Use `asyncio.run(...)` when running in a script.
async def main():
    # result = await team.run(task="Write a short poem about the fall season.")
    # print(result)
    #await team.reset()  # Reset the team for a new task.
    # async for message in team.run_stream(task="Write a short poem about the fall season."):  # type: ignore
    #     if isinstance(message, TaskResult):
    #         print("Stop Reason:", message.stop_reason)
    #     else:
    #         print(message)
    # await team.reset()  # Reset the team for a new task.
    await Console(team.run_stream(task="Write a short poem about the fall season."))  # Stream the messages to the console.


asyncio.run(main())
