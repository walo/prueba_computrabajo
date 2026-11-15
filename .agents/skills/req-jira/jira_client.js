const fs = require("fs");
const path = require("path");
const https = require("https");

function loadEnv() {
  const envPath = path.join(process.cwd(), ".env");
  if (!fs.existsSync(envPath)) return {};
  const content = fs.readFileSync(envPath, "utf-8");
  const config = {};
  for (const line of content.split("\n")) {
    const trimmed = line.trim();
    if (trimmed && !trimmed.startsWith("#") && trimmed.includes("=")) {
      const idx = trimmed.indexOf("=");
      config[trimmed.substring(0, idx).trim()] = trimmed
        .substring(idx + 1)
        .trim();
    }
  }
  return config;
}

function getAuthContext() {
  const config = loadEnv();
  const baseUrl = (config.JIRA_BASE_URL || "").replace(/\/$/, "");
  const email = config.JIRA_USER_EMAIL;
  const token = config.JIRA_API_TOKEN;
  const projectKey = process.env.JIRA_PROJECT_KEY || config.JIRA_PROJECT_KEY || "PROJ";

  if (!email || !token || !baseUrl) {
    console.error(JSON.stringify({ error: "Faltan variables en .env" }));
    process.exit(1);
  }

  const auth = Buffer.from(`${email}:${token}`).toString("base64");
  const hostname = baseUrl.replace(/^https?:\/\//, "");

  return { baseUrl, hostname, auth, projectKey };
}

function parseAdfToText(node) {
  if (!node) return "";
  if (node.type === "text") return node.text || "";
  if (Array.isArray(node.content)) {
    const inner = node.content.map(parseAdfToText).join("");
    if (node.type === "paragraph") return inner + "\n";
    return inner;
  }
  return "";
}

function createIssue(summary, description, issueType = "Story") {
  const { baseUrl, hostname, auth, projectKey } = getAuthContext();

  let finalDesc = description;
  if (description.startsWith("@") && fs.existsSync(description.slice(1))) {
    finalDesc = fs.readFileSync(description.slice(1), "utf-8");
  }

  // Map Story to Historia if Jira is localized in Spanish
  let resolvedIssueType = issueType;
  if (resolvedIssueType === "Story") {
    resolvedIssueType = "Historia";
  }

  const adfParagraphs = finalDesc.split("\n").map((line) => {
    return line.trim()
      ? { type: "paragraph", content: [{ type: "text", text: line }] }
      : { type: "paragraph", content: [] };
  });

  const payload = JSON.stringify({
    fields: {
      project: { key: projectKey },
      summary: summary,
      description: {
        type: "doc",
        version: 1,
        content: adfParagraphs,
      },
      issuetype: { name: resolvedIssueType },
    },
  });

  const options = {
    hostname: hostname,
    path: "/rest/api/3/issue",
    method: "POST",
    headers: {
      Authorization: `Basic ${auth}`,
      "Content-Type": "application/json",
      Accept: "application/json",
      "Content-Length": Buffer.byteLength(payload),
    },
  };

  const req = https.request(options, (res) => {
    let data = "";
    res.on("data", (chunk) => (data += chunk));
    res.on("end", () => {
      if (res.statusCode >= 200 && res.statusCode < 300) {
        const parsed = JSON.parse(data);
        console.log(
          JSON.stringify({
            status: "success",
            key: parsed.key,
            url: `${baseUrl}/browse/${parsed.key}`,
          }),
        );
      } else {
        console.error(
          JSON.stringify({
            status: "error",
            code: res.statusCode,
            response: data,
          }),
        );
        process.exit(1);
      }
    });
  });

  req.on("error", (e) => {
    console.error(
      JSON.stringify({ status: "network_error", message: e.message }),
    );
    process.exit(1);
  });

  req.write(payload);
  req.end();
}

function getIssue(issueKey) {
  const { baseUrl, hostname, auth } = getAuthContext();

  const options = {
    hostname: hostname,
    path: `/rest/api/3/issue/${issueKey}?fields=summary,description,status,issuetype`,
    method: "GET",
    headers: {
      Authorization: `Basic ${auth}`,
      Accept: "application/json",
    },
  };

  const req = https.request(options, (res) => {
    let data = "";
    res.on("data", (chunk) => (data += chunk));
    res.on("end", () => {
      if (res.statusCode >= 200 && res.statusCode < 300) {
        const parsed = JSON.parse(data);
        const descRaw = parsed.fields.description;
        const textDescription = descRaw ? parseAdfToText(descRaw).trim() : "";

        console.log(
          JSON.stringify(
            {
              status: "success",
              key: parsed.key,
              summary: parsed.fields.summary,
              description: textDescription,
              issueType: parsed.fields.issuetype.name,
              issueStatus: parsed.fields.status.name,
            },
            null,
            2,
          ),
        );
      } else {
        console.error(
          JSON.stringify({
            status: "error",
            code: res.statusCode,
            response: data,
          }),
        );
        process.exit(1);
      }
    });
  });

  req.on("error", (e) => {
    console.error(
      JSON.stringify({ status: "network_error", message: e.message }),
    );
    process.exit(1);
  });

  req.end();
}

function updateIssue(issueKey, description, summary = null) {
  const { baseUrl, hostname, auth } = getAuthContext();

  let finalDesc = description;
  if (description.startsWith("@") && fs.existsSync(description.slice(1))) {
    finalDesc = fs.readFileSync(description.slice(1), "utf-8");
  }

  const adfParagraphs = finalDesc.split("\n").map((line) => {
    return line.trim()
      ? { type: "paragraph", content: [{ type: "text", text: line }] }
      : { type: "paragraph", content: [] };
  });

  const fields = {
    description: {
      type: "doc",
      version: 1,
      content: adfParagraphs,
    },
  };

  if (summary) {
    fields.summary = summary;
  }

  const payload = JSON.stringify({ fields });

  const options = {
    hostname: hostname,
    path: `/rest/api/3/issue/${issueKey}`,
    method: "PUT",
    headers: {
      Authorization: `Basic ${auth}`,
      "Content-Type": "application/json",
      Accept: "application/json",
      "Content-Length": Buffer.byteLength(payload),
    },
  };

  const req = https.request(options, (res) => {
    let data = "";
    res.on("data", (chunk) => (data += chunk));
    res.on("end", () => {
      if (res.statusCode >= 200 && res.statusCode < 300) {
        console.log(
          JSON.stringify({
            status: "success",
            key: issueKey,
            url: `${baseUrl}/browse/${issueKey}`,
            message: "Issue actualizado correctamente",
          }),
        );
      } else {
        console.error(
          JSON.stringify({
            status: "error",
            code: res.statusCode,
            response: data,
          }),
        );
        process.exit(1);
      }
    });
  });

  req.on("error", (e) => {
    console.error(
      JSON.stringify({ status: "network_error", message: e.message }),
    );
    process.exit(1);
  });

  req.write(payload);
  req.end();
}

function transitionIssue(issueKey, targetStateName) {
  const { baseUrl, hostname, auth } = getAuthContext();

  // 1. Get available transitions
  const getOptions = {
    hostname: hostname,
    path: `/rest/api/3/issue/${issueKey}/transitions`,
    method: "GET",
    headers: {
      Authorization: `Basic ${auth}`,
      Accept: "application/json",
    },
  };

  const getReq = https.request(getOptions, (res) => {
    let data = "";
    res.on("data", (chunk) => (data += chunk));
    res.on("end", () => {
      if (res.statusCode >= 200 && res.statusCode < 300) {
        const parsed = JSON.parse(data);
        const transitions = parsed.transitions || [];

        // Find transition by name (case-insensitive and trimmed)
        const targetClean = targetStateName.toLowerCase().trim();
        const found = transitions.find(
          (t) =>
            t.name.toLowerCase().trim() === targetClean ||
            (t.to && t.to.name.toLowerCase().trim() === targetClean)
        );

        if (!found) {
          console.error(
            JSON.stringify({
              status: "error",
              message: `No se encontró la transición "${targetStateName}".`,
              availableTransitions: transitions.map((t) => ({
                id: t.id,
                name: t.name,
                to: t.to ? t.to.name : undefined,
              })),
            })
          );
          process.exit(1);
        }

        // 2. Perform transition
        const postPayload = JSON.stringify({
          transition: { id: found.id },
        });

        const postOptions = {
          hostname: hostname,
          path: `/rest/api/3/issue/${issueKey}/transitions`,
          method: "POST",
          headers: {
            Authorization: `Basic ${auth}`,
            "Content-Type": "application/json",
            Accept: "application/json",
            "Content-Length": Buffer.byteLength(postPayload),
          },
        };

        const postReq = https.request(postOptions, (postRes) => {
          let postData = "";
          postRes.on("data", (chunk) => (postData += chunk));
          postRes.on("end", () => {
            if (postRes.statusCode >= 200 && postRes.statusCode < 300) {
              console.log(
                JSON.stringify({
                  status: "success",
                  key: issueKey,
                  transition: found.name,
                  targetState: found.to ? found.to.name : targetStateName,
                  message: `Estado transicionado exitosamente a "${targetStateName}".`,
                })
              );
            } else {
              console.error(
                JSON.stringify({
                  status: "error",
                  code: postRes.statusCode,
                  response: postData,
                })
              );
              process.exit(1);
            }
          });
        });

        postReq.on("error", (e) => {
          console.error(
            JSON.stringify({ status: "network_error", message: e.message })
          );
          process.exit(1);
        });

        postReq.write(postPayload);
        postReq.end();
      } else {
        console.error(
          JSON.stringify({
            status: "error",
            code: res.statusCode,
            response: data,
          })
        );
        process.exit(1);
      }
    });
  });

  getReq.on("error", (e) => {
    console.error(
      JSON.stringify({ status: "network_error", message: e.message })
    );
    process.exit(1);
  });

  getReq.end();
}

const args = process.argv.slice(2);
const command = args[0];

if (command === "create") {
  createIssue(args[1], args[2] || "", args[3] || "Story");
} else if (command === "update") {
  if (!args[1] || !args[2]) {
    console.error("Uso: node jira_client.js update <issue_key> <description> [summary]");
    process.exit(1);
  }
  updateIssue(args[1], args[2], args[3] || null);
} else if (command === "get") {
  if (!args[1]) {
    console.error("Uso: node jira_client.js get <issue_key>");
    process.exit(1);
  }
  getIssue(args[1]);
} else if (command === "transition") {
  if (!args[1] || !args[2]) {
    console.error("Uso: node jira_client.js transition <issue_key> <target_state_name>");
    process.exit(1);
  }
  transitionIssue(args[1], args[2]);
} else {
  console.log(
    'Uso:\n  node jira_client.js create "<summary>" "<description>" [issue_type]\n  node jira_client.js update <issue_key> "<description>" [summary]\n  node jira_client.js get <issue_key>\n  node jira_client.js transition <issue_key> "<target_state_name>"',
  );
  process.exit(1);
}

