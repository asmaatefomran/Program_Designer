import type { BuilderGroup, BuilderStep } from "@/builder/tree";

function step(templateId: string, name: string, prerequisiteTemplateIds: string[] = []): BuilderStep {
  return {
    kind: "step",
    key: templateId,
    templateId,
    name,
    prerequisiteTemplateIds,
  };
}

export function buildComputerScienceScenario(): BuilderGroup {
  const foundations: BuilderGroup = {
    kind: "group",
    key: "group-foundations",
    templateId: "group-foundations",
    name: "Foundations",
    groupType: "All",
    requiredChoiceCount: 0,
    prerequisiteTemplateIds: [],
    children: [
      step("step-intro", "Introduction to Computing"),
      step("step-math", "Mathematics for Computing"),
    ],
  };

  const electives: BuilderGroup = {
    kind: "group",
    key: "group-electives",
    templateId: "group-electives",
    name: "Electives",
    groupType: "Choice",
    requiredChoiceCount: 2,
    prerequisiteTemplateIds: [],
    children: [
      step("step-cv", "Computer Vision"),
      step("step-nlp", "Natural Language Processing"),
      step("step-robotics", "Robotics"),
    ],
  };

  const aiTrack: BuilderGroup = {
    kind: "group",
    key: "group-ai",
    templateId: "group-ai",
    name: "AI",
    groupType: "All",
    requiredChoiceCount: 0,
    prerequisiteTemplateIds: [],
    children: [
      step("step-ml-basics", "Machine Learning Basics"),
      electives,
      step("step-ai-capstone", "AI Capstone", ["group-electives"]),
    ],
  };

  const itTrack: BuilderGroup = {
    kind: "group",
    key: "group-it",
    templateId: "group-it",
    name: "IT",
    groupType: "All",
    requiredChoiceCount: 0,
    prerequisiteTemplateIds: [],
    children: [
      step("step-networks", "Networks & Security"),
      step("step-sysadmin", "Systems Administration"),
    ],
  };

  const programmingTrack: BuilderGroup = {
    kind: "group",
    key: "group-programming",
    templateId: "group-programming",
    name: "Programming",
    groupType: "All",
    requiredChoiceCount: 0,
    prerequisiteTemplateIds: [],
    children: [
      step("step-algorithms", "Algorithms & Data Structures"),
      step("step-swe", "Software Engineering"),
    ],
  };

  const major: BuilderGroup = {
    kind: "group",
    key: "group-major",
    templateId: "group-major",
    name: "Major",
    groupType: "Choice",
    requiredChoiceCount: 1,
    prerequisiteTemplateIds: ["group-foundations"],
    children: [aiTrack, itTrack, programmingTrack],
  };

  const finalCapstone = step("step-final-capstone", "Final Capstone", ["group-major"]);

  return {
    kind: "group",
    key: "group-root",
    templateId: "group-root",
    name: "Computer Science",
    groupType: "All",
    requiredChoiceCount: 0,
    prerequisiteTemplateIds: [],
    children: [foundations, major, finalCapstone],
  };
}
