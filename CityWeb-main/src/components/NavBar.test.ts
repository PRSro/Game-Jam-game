import { describe, expect, it, afterEach } from "vitest";
import { h } from "vue";
import { mount } from "@vue/test-utils";
import { createMemoryHistory, createRouter } from "vue-router";
import NavBar from "./NavBar.vue";
import { setLocale } from "../i18n/index";

const stub = { render: () => h("div") };

async function mountNav() {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: "/evenimente", component: stub },
      { path: "/calendar", component: stub },
    ],
  });
  await router.push("/evenimente");
  await router.isReady();
  const wrapper = mount(NavBar, { global: { plugins: [router] } });
  return { wrapper, router };
}

afterEach(() => setLocale("ro"));

describe("NavBar", () => {
  it("renders one link per route with the right href", async () => {
    const { wrapper } = await mountNav();
    const links = wrapper.findAll("a");

    expect(links).toHaveLength(3);
    expect(links[0].attributes("href")).toBe("/evenimente");
    expect(links[0].text()).toBe("Evenimente");
    expect(links[1].attributes("href")).toBe("/calendar");
    expect(links[2].attributes("href")).toBe("/harta");
  });

  it("marks the active route with aria-current", async () => {
    const { wrapper, router } = await mountNav();

    expect(wrapper.findAll("a")[0].attributes("aria-current")).toBe("page");
    expect(wrapper.findAll("a")[1].attributes("aria-current")).toBeUndefined();

    await router.push("/calendar");
    expect(wrapper.findAll("a")[1].attributes("aria-current")).toBe("page");
  });

  it("switches locale from the nav", async () => {
    const { wrapper } = await mountNav();
    const buttons = wrapper.findAll("button");

    expect(buttons).toHaveLength(2);
    await buttons[1].trigger("click");

    expect(wrapper.findAll("a")[0].text()).toBe("Events");
    expect(buttons[1].attributes("aria-pressed")).toBe("true");
  });
});
