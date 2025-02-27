import OpenAI from "openai";
const openai = new OpenAI();

const completion = await openai.chat.completions.create({
    model: "gpt-4o-mini",
    messages: [
        { role: "system", content: "You are a helpful assistant." },
        {
            role: "user",
            // for content within ' ' needs to use Response from Sent Feedback - select with <p>.childNodes[i] (?)
            content: "Can you analyze the results of 'The varibles named were vauge' and provide recommended methods to implement provided feedback?",
        },
    ],
    store: true,
});

console.log(completion.choices[0].message);


const displayMessage = document.querySelector("#notifBox");
for (let i = 0; i < choices.length; i++) {
    displayMessage.innerHTML += `${completion.choices[0].message}`;
}


