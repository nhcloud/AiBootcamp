import asyncio
import os
import requests
from dotenv import load_dotenv
from autogen_agentchat.agents import AssistantAgent
from autogen_agentchat.ui import Console
from autogen_ext.models.openai import OpenAIChatCompletionClient
from autogen_ext.models.openai import AzureOpenAIChatCompletionClient
from autogen_agentchat.messages import TextMessage
from io import BytesIO
from autogen_core import CancellationToken
from autogen_core.tools import FunctionTool

from autogen_agentchat.messages import MultiModalMessage
from autogen_core import Image as AGImage
from PIL import Image
load_dotenv()

pil_image = Image.open(BytesIO(requests.get("https://picsum.photos/300/200").content))
img = AGImage(pil_image)
multi_modal_message = MultiModalMessage(content=["Can you describe the content of this image?", img], source="User")
print(multi_modal_message.content)  # Output: "Can you describe the content of this image?"
img
#print(multi_modal_message.images)  # Output: [Image]
# Load the environment variables from the .env file.
text_message = TextMessage(content="Hello, world!", source="User")
#print(text_message.content)  # Output: "Hello, world!"
model_client = AzureOpenAIChatCompletionClient(
    azure_deployment=os.getenv("DEPLOYMENT_NAME"),
    model=os.getenv("MODEL_NAME"),
    api_version=os.getenv("API_VERSION"),
    azure_endpoint=os.getenv("ENDPOINT_URI"),
    api_key=os.getenv("API_KEY")
)

# Define a model client. You can use other model client that implements
# the `ChatCompletionClient` interface.
# model_client = OpenAIChatCompletionClient(
#     model=os.getenv("MODEL_NAME"),
#     api_key=os.getenv("OPEN_AI_API_KEY")
# )



# Define an AssistantAgent with the model, tool, system message, and reflection enabled.
# The system message instructs the agent via natural language.
agent = AssistantAgent(
    name="assistant",
    model_client=model_client,
    system_message="You are a helpful assistant.",
    reflect_on_tool_use=True,
    model_client_stream=True,  # Enable streaming tokens from the model client.
)


# NOTE: if running this inside a Python script you'll need to use asyncio.run(main()).
#asyncio.run(main3())

async def agent_run_text_message() -> None:
    response = await agent.on_messages(
        [TextMessage(content="Find information on AutoGen", source="user")],
        cancellation_token=CancellationToken(),
    )
    print(response.inner_messages)
    print(response.chat_message)

async def agent_run_multimodel_message() -> None:
    response = await agent.on_messages([multi_modal_message], CancellationToken())
    print(response.chat_message.content)

async def agent_run_text_stream() -> None:
    # Option 1: read each message from the stream (as shown in the previous example).
    # async for message in agent.on_messages_stream(
    #     [TextMessage(content="Find information on AutoGen", source="user")],
    #     cancellation_token=CancellationToken(),
    # ):
    #     print(message)

    # Option 2: use Console to print all messages as they appear.
    await Console(
        agent.on_messages_stream(
            [TextMessage(content="Find information on AutoGen", source="user")],
            cancellation_token=CancellationToken(),
        ),
        output_stats=True,  # Enable stats printing.
    )

async def agent_run_multimodel_stream() -> None:
    await Console(
        agent.on_messages_stream(
            [multi_modal_message],
            cancellation_token=CancellationToken(),
        ),
        output_stats=True,  # Enable stats printing.
    )


while True:
    print("Choose an option:")
    print("1. Run agent with text message")
    print("2. Run agent with multimodal message")
    print("3. Run agent with text stream")
    print("4. Run agent with multimodal stream")
    print("0. Exit")

    choice = input("Enter your choice: ")

    if choice == "1":
        asyncio.run(agent_run_text_message())
    elif choice == "2":
        asyncio.run(agent_run_multimodel_message())
    elif choice == "3":
        asyncio.run(agent_run_text_stream())
    elif choice == "4":
        asyncio.run(agent_run_multimodel_stream())
    elif choice == "0":
        break
    else:
        print("Invalid choice, please try again.")


