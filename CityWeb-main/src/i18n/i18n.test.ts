import { describe, expect, it, afterEach } from "vitest";
import { t, tm, setLocale, locale } from "./index";

afterEach(() => setLocale("ro"));

describe("i18n", () => {
  it("defaults to Romanian", () => {
    expect(locale.value).toBe("ro");
    expect(t("nav.events")).toBe("Evenimente");
  });

  it("returns the key when a message is missing", () => {
    expect(t("does.not.exist")).toBe("does.not.exist");
  });

  it("interpolates params", () => {
    expect(t("events.count", { n: 3 })).toBe("3 evenimente");
    expect(t("events.countOne", { n: 1 })).toBe("1 eveniment");
  });

  it("leaves unknown placeholders untouched", () => {
    expect(t("events.count", { other: 1 })).toBe("{n} evenimente");
  });

  it("exposes list messages only through tm", () => {
    expect(tm("cal.months")).toHaveLength(12);
    expect(tm("cal.weekdays")).toEqual(["L", "M", "M", "J", "V", "S", "D"]);
    expect(t("cal.months")).toBe("");
    expect(tm("cal.today")).toEqual([]);
  });

  it("switches locale and persists it", () => {
    setLocale("en");
    expect(t("nav.events")).toBe("Events");
    expect(localStorage.getItem("piata.locale")).toBe("en");

    setLocale("ro");
    expect(t("nav.events")).toBe("Evenimente");
  });
});
