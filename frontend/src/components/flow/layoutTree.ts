import type { BuilderNode } from "@/builder/tree";

export interface LayoutPosition {
  x: number;
  y: number;
}

const COLUMN_WIDTH = 240;
const ROW_HEIGHT = 120;

/** Assigns each node an (x, y) position: y by depth, x by in-order leaf position. */
export function layoutTree(root: BuilderNode): Map<string, LayoutPosition> {
  const positions = new Map<string, LayoutPosition>();
  let leafIndex = 0;

  function visit(node: BuilderNode, depth: number): number {
    if (node.kind === "step" || node.children.length === 0) {
      const x = leafIndex;
      leafIndex += 1;
      positions.set(node.key, { x: x * COLUMN_WIDTH, y: depth * ROW_HEIGHT });
      return x;
    }

    const childXs = node.children.map((c) => visit(c, depth + 1));
    const x = childXs.reduce((a, b) => a + b, 0) / childXs.length;
    positions.set(node.key, { x: x * COLUMN_WIDTH, y: depth * ROW_HEIGHT });
    return x;
  }

  visit(root, 0);
  return positions;
}
