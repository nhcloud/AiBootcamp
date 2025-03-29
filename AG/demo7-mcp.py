import asyncio
import os

from dotenv import load_dotenv
from autogen_agentchat.agents import AssistantAgent
from autogen_ext.models.openai import OpenAIChatCompletionClient
from autogen_ext.models.openai import AzureOpenAIChatCompletionClient
from autogen_ext.tools.mcp import StdioServerParams, mcp_server_tools

load_dotenv()

model_client = AzureOpenAIChatCompletionClient(
    azure_deployment=os.getenv("DEPLOYMENT_NAME"),
    model=os.getenv("MODEL_NAME"),
    api_version=os.getenv("API_VERSION"),
    azure_endpoint=os.getenv("ENDPOINT_URI"),
    api_key=os.getenv("API_KEY")
)
# Get the fetch tool from mcp-server-fetch.
fetch_mcp_server = StdioServerParams(command="uvx", args=["mcp-server-fetch"])
async def main():
    tools = await mcp_server_tools(fetch_mcp_server)

    # Create an agent that can use the fetch tool.
    model_client = OpenAIChatCompletionClient(model="gpt-4o")
    agent = AssistantAgent(name="fetcher", model_client=model_client, tools=tools, reflect_on_tool_use=True)  # type: ignore

    # Let the agent fetch the content of a URL and summarize it.
    result = await agent.run(task="Summarize the content of https://en.wikipedia.org/wiki/Seattle")
    print(result.messages[-1].content)

# Run the async main function
import asyncio
asyncio.run(main())
