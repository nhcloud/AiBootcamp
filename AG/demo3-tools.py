import asyncio
import os
from dotenv import load_dotenv
from autogen_agentchat.agents import AssistantAgent
from autogen_agentchat.ui import Console
from autogen_ext.models.openai import OpenAIChatCompletionClient
from autogen_ext.models.openai import AzureOpenAIChatCompletionClient
from autogen_ext.models.ollama import OllamaChatCompletionClient  # Import Ollama client
from autogen_agentchat.messages import TextMessage
from io import BytesIO
from autogen_core import CancellationToken
from autogen_core.tools import FunctionTool
load_dotenv()
model_client = AzureOpenAIChatCompletionClient(
    azure_deployment=os.getenv("DEPLOYMENT_NAME"),
    model=os.getenv("MODEL_NAME"),
    api_version=os.getenv("API_VERSION"),
    azure_endpoint=os.getenv("ENDPOINT_URI"),
    api_key=os.getenv("API_KEY")
)

model_client = OpenAIChatCompletionClient(
    model=os.getenv("MODEL_NAME"),
    api_key=os.getenv("OPEN_AI_API_KEY")
)

async def get_weather(city: str) -> str:
    """Get the weather for a given city."""
    return f"The weather in {city} is 120 degrees and Sunny."

# Define a tool using a Python function.
async def web_search_func(query: str) -> str:
    """Find information on the web"""
    return "Udaiapa Ramachandran shortly called Udai."

web_search_function_tool = FunctionTool(web_search_func, description="Find the person details tool")
# The schema is provided to the model during AssistantAgent's on_messages call.
#web_search_function_tool.schema
agent = AssistantAgent(
    name="assistant",
    model_client=model_client,
    tools=[get_weather],
    system_message="You are a helpful assistant.",
    reflect_on_tool_use=True,
    model_client_stream=True,  # Enable streaming tokens from the model client.
)


async def summarize_udai_io():
    result = await agent.run(task="write about boston and include weather")
    print(result.messages[0].content)
    print(result.messages[-1].content)

# Call the function using asyncio.run()
asyncio.run(summarize_udai_io())