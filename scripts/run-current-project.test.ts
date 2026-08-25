import { afterEach, describe, expect, test } from "bun:test";
import { mkdir, mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { findCurrentProject } from "./run-current-project";

const temporaryDirectories: string[] = [];

async function projectsFixture(...names: string[]) {
  const root = await mkdtemp(join(tmpdir(), "internship-projects-"));
  temporaryDirectories.push(root);
  await Promise.all(names.map((name) => mkdir(join(root, name))));
  return root;
}

afterEach(async () => {
  await Promise.all(temporaryDirectories.splice(0).map((directory) => rm(directory, { recursive: true })));
});

describe("current project detection", () => {
  test("ignores the template and selects the branch project", async () => {
    const projects = await projectsFixture("_template", "inventory-app");
    expect(await findCurrentProject(projects)).toBe(join(projects, "inventory-app"));
  });

  test("selects a different project without changing root configuration", async () => {
    const projects = await projectsFixture("_template", "calculator-app");
    expect(await findCurrentProject(projects)).toBe(join(projects, "calculator-app"));
  });

  test("reports no active project clearly", async () => {
    const projects = await projectsFixture("_template");
    expect(findCurrentProject(projects)).rejects.toThrow("No active project found");
  });

  test("rejects ambiguous branches", async () => {
    const projects = await projectsFixture("_template", "one-app", "two-app");
    expect(findCurrentProject(projects)).rejects.toThrow("Expected exactly one active project");
  });

  test("enforces lowercase-kebab-case project names", async () => {
    const projects = await projectsFixture("_template", "InventoryApp");
    expect(findCurrentProject(projects)).rejects.toThrow("lowercase-kebab-case");
  });
});
